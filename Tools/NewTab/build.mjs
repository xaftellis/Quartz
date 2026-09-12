// Builds Chromium WebUI components for Quartz. Original downloaded files are
// retained byte-for-byte; only generated import wrappers and host APIs differ.
import * as esbuild from 'esbuild';
import fs from 'node:fs/promises';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import crypto from 'node:crypto';

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, '../..');
const vendor = path.join(root, 'chromeium/webui');
const output = path.join(root, 'Quartz/assets/quartz.com/newtab');
const revision = 'a2bee684f3c4224a8836957b917c167d9bb9a349';
const online = process.argv.includes('--sync');
const manifest = {};
const inflight = new Map();

async function source(name) {
  if (inflight.has(name)) return inflight.get(name);
  const pending = (async () => {
    const file = path.join(vendor, name);
    let bytes;
    try { bytes = await fs.readFile(file); }
    catch {
      if (!online) throw new Error(`Missing vendored source: ${name}. Run node build.mjs --sync.`);
      const response = await fetch(`https://raw.githubusercontent.com/chromium/chromium/${revision}/${name}`);
      if (!response.ok) throw new Error(`Upstream ${response.status}: ${name}`);
      bytes = Buffer.from(await response.arrayBuffer());
      await fs.mkdir(path.dirname(file), {recursive:true});
      await fs.writeFile(file, bytes);
    }
    manifest[name] = crypto.createHash('sha256').update(bytes).digest('hex');
    return bytes.toString('utf8');
  })();
  inflight.set(name, pending);
  return pending;
}

function platform(text) {
  // Match Chromium's preprocessing for a desktop Windows build.
  const flags = {is_win:true, is_linux:false, is_macosx:false, is_chromeos:false, is_android:false,
    is_ios:false, chromeos_ash:false, chromeos_lacros:false, is_official_build:true,
    use_blink:true, optimize_webui:true, is_chrome_branded:true};
  const stack = [true];
  let result = '', from = 0;
  const tags = /<if expr="([^"]+)">|<\/if>/g;
  for (const match of text.matchAll(tags)) {
    if (stack.at(-1)) result += text.slice(from, match.index);
    if (match[1]) {
      let expr = match[1].replace(/\b(and|or|not)\b/g, x => ({and:'&&',or:'||',not:'!'}[x]));
      expr = expr.replace(/\b[a-zA-Z_]\w*\b/g, x => String(flags[x] ?? false));
      stack.push(stack.at(-1) && Function(`return (${expr})`)());
    } else stack.pop();
    from = match.index + match[0].length;
  }
  if (stack.at(-1)) result += text.slice(from);
  return result;
}

function resource(spec, importer = '') {
  if (/^(chrome:)?\/\/resources\//.test(spec)) return spec.replace(/^(chrome:)?\/\/resources\//, 'ui/webui/resources/');
  if (spec.startsWith('chrome://new-tab-page/')) return spec.replace('chrome://new-tab-page/', 'chrome/browser/resources/new_tab_page/');
  return path.posix.normalize(path.posix.join(path.posix.dirname(importer), spec));
}

const assets = new Map();
const generatedAssets = new Set();
async function asset(name) {
  if (!assets.has(name)) {
    const text = await source(name);
    const relative = 'chromium/' + name.replace(/^ui\/webui\/resources\//, '');
    const file = path.join(output, relative);
    await fs.mkdir(path.dirname(file), {recursive:true});
    await fs.writeFile(file, text);
    assets.set(name, relative);
    generatedAssets.add(relative);
  }
  return assets.get(name);
}

async function cssText(name) {
  let css = platform(await source(name));
  const matches = [...css.matchAll(/url\((['"]?)([^)'"\s]+)\1\)/g)];
  for (const match of matches) {
    if (match[2].startsWith('data:') || match[2].startsWith('#')) continue;
    const destination = await asset(resource(match[2], name));
    css = css.replace(match[0], `url("/newtab/${destination}")`);
  }
  return css;
}

const plugin = {
  name:'chromium-webui',
  setup(build) {
    build.onResolve({filter:/.*/}, args => {
      if (args.path.startsWith('lit') || args.path === '@lit/reactive-element') return {path:args.path.replace('lit/index.js','lit').replace('lit-html/','lit/'), external:true};
      if (args.path === 'quartz-host') return {path:path.join(here, 'host.ts')};
      const isSource = args.namespace === 'chromium' || /^(chrome:)?\/\/resources\//.test(args.path) || args.path.startsWith('chrome://new-tab-page/');
      if (!isSource) return;
      let name = resource(args.path, args.importer);
      if (name === 'ui/webui/resources/lit/v3_0/lit.rollup.js') name = 'third_party/lit/v3_0/lit.ts';
      if (name.endsWith('/most_visited.mojom-webui.js') || name.endsWith('/searchbox_browser_proxy.js') ||
          name.includes('/metrics_reporter/') || name.endsWith('/load_time_data.js') || name.includes('/mojo/'))
        return {path:path.join(here,'host.ts')};
      return {path:name, namespace:'chromium'};
    });
    build.onLoad({filter:/.*/, namespace:'chromium'}, async args => {
      let name = args.path;
      if (name.endsWith('.css.js')) {
        name = name.slice(0, -3);
        const css = await cssText(name);
        const imported = [...css.matchAll(/#import=(\S+)/g)].map(m => m[1]);
        const isVars = /#type=vars-lit/.test(css);
        if (isVars) {
          const relative = 'chromium/' + name.replace(/^ui\/webui\/resources\//, '');
          await fs.mkdir(path.dirname(path.join(output,relative)),{recursive:true});
          await fs.writeFile(path.join(output,relative),css);
          generatedAssets.add(relative);
          return {loader:'js', contents:`const style=document.createElement('link');style.rel='stylesheet';style.href=${JSON.stringify('/newtab/'+relative)};document.head.append(style);export function getCss(){return []}`};
        }
        const dependencies = imported.filter(x => !x.includes('cr_shared_vars'));
        const imports = imported.map((x,i) => dependencies.includes(x) ? `import {getCss as css${i}} from ${JSON.stringify(x)};` : `import ${JSON.stringify(x)};`).join('\n');
        return {loader:'js', contents:`import {css} from 'lit';${imports}\nexport function getCss(){return [${imported.map((x,i)=>dependencies.includes(x)?`css${i}(),`:'').join('')}css([${JSON.stringify(css)}])]}`};
      }
      if (name.endsWith('.html.js')) {
        const base = name.slice(0,-3);
        let html;
        try { html = await source(base + '.ts'); name = base + '.ts'; }
        catch { html = await source(base); name = base; }
        html = platform(html);
        if (!name.endsWith('.ts'))
          html = `import {html} from 'lit';export function getHtml(){return html\`${html}\`;}`;
        return {loader:'ts',contents:html};
      }
      if (name.endsWith('.js')) name = name.slice(0,-3) + '.ts';
      let text = platform(await source(name));
      if (name === 'third_party/lit/v3_0/lit.ts')
        text = text.replace('css, CSSResultGroup, html, LitElement, nothing, render, PropertyValues, TemplateResult','css, html, LitElement, nothing, render').replace('directive, PartInfo, PartType','directive, PartType');
      if (name.endsWith('/searchbox_icon.ts'))
        text = text.replace("from '//resources/js/icon.js'", "from 'quartz-host'");
      // Chromium's favicon2 URL is a browser service, supplied by Quartz instead.
      if (name.endsWith('/most_visited.ts')) {
        text = `import {faviconUrl} from 'quartz-host';\n` + text;
        const start = text.indexOf('  protected getFaviconUrl_(');
        const end = text.indexOf('\n  protected ', start + 1);
        text = text.slice(0,start) + '  protected getFaviconUrl_(url) { return faviconUrl(url); }\n' + text.slice(end);
      }
      return {loader:'ts', contents:text};
    });
  }
};

await fs.mkdir(output,{recursive:true});
await fs.copyFile(path.join(here,'node_modules/lit/LICENSE'),path.join(output,'lit-LICENSE'));
for (const name of ['search_cr23.svg','history_cr23.svg','page_cr23.svg'])
  await asset('ui/webui/resources/cr_components/searchbox/icons/'+name);
// Reproduce the Windows WebUI text defaults, including the original 81.25% body size.
const defaults=platform(await source('ui/webui/resources/css/text_defaults_md.css'))
  .replaceAll('$i18nRaw{fontfamilyMd}', "'Segoe UI', Tahoma, sans-serif");
await fs.writeFile(path.join(output,'chromium-text-defaults.css'),defaults);
// Stage one resolves Chromium's generated resource URLs; stage two bundles Lit
// from the lockfile, leaving no runtime CDN or network dependency.
const stage = await esbuild.build({entryPoints:[path.join(here,'entry.ts')],bundle:true,write:false,format:'esm',target:'es2022',plugins:[plugin],logLevel:'warning'});
await esbuild.build({stdin:{contents:stage.outputFiles[0].text,resolveDir:here,sourcefile:'chromium-components.js'},bundle:true,format:'esm',target:'es2022',outfile:path.join(output,'chromium-components.js'),legalComments:'eof',minify:false,logLevel:'warning'});
await fs.writeFile(path.join(vendor,'manifest.json'), JSON.stringify({revision,files:manifest},null,2)+'\n');
// Remove obsolete generated assets only after the replacement bundle succeeds.
async function pruneGenerated(directory) {
  for (const entry of await fs.readdir(directory, {withFileTypes:true})) {
    const file = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      await pruneGenerated(file);
      if (!(await fs.readdir(file)).length) await fs.rmdir(file);
    } else if (entry.isFile() && !generatedAssets.has(path.relative(output,file).split(path.sep).join('/'))) {
      await fs.unlink(file);
    }
  }
}
await pruneGenerated(path.join(output,'chromium'));
console.log(`Built ${Object.keys(manifest).length} original Chromium sources and ${assets.size} original assets at ${revision}.`);


