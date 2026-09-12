using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Quartz.Libs;
using Quartz.Models;
using Quartz.Omnibox;
using Quartz.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

internal static class NewTabTests
{
    private static WebView2 view;
    private static int checks;
    private static string output, destination, theme = "light";
    private static bool online;
    private static readonly Stopwatch clock = Stopwatch.StartNew();
    private static readonly List<long> requestStarts = new List<long>();
    private static readonly Guid profile = Guid.NewGuid();
    private static readonly List<HistoryModel> history = new List<HistoryModel> {
        new HistoryModel { Id=Guid.NewGuid(), ProfileId=profile, Title="Recent Quartz page", WebAddress="https://example.org/recent", When=DateTime.Now },
        new HistoryModel { Id=Guid.NewGuid(), ProfileId=profile, Title="Earlier page", WebAddress="https://example.org/older", When=DateTime.Now.AddHours(-1) },
        new HistoryModel { Id=Guid.NewGuid(), ProfileId=Guid.NewGuid(), Title="OTHER PROFILE", WebAddress="https://example.net/", When=DateTime.Now }
    };
    private static void Check(bool value,string name){if(!value)throw new Exception("FAIL: "+name);checks++;Console.WriteLine("PASS: "+name);}
    private static Task<string> JS(string script){return view.CoreWebView2.ExecuteScriptAsync(script);}
    private static async Task Assert(string expression,string name){Check(await JS(expression)=="true",name);}
    private static async Task Until(string expression,int timeout=5000){
        var end=DateTime.UtcNow.AddMilliseconds(timeout);
        do{if(await JS(expression)=="true")return;await Task.Delay(30);}while(DateTime.UtcNow<end);
        throw new Exception("Timeout: "+expression+"\nJS errors: "+await JS("JSON.stringify(window.__errors)"));
    }
    private static async Task Capture(string name){
        await Task.Delay(100);
        using(var stream=File.Create(Path.Combine(output,name+".png")))
            await view.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png,stream);
    }
    private static async Task Bind(){
        await Until("!!document.querySelector('ntp-app')?.shadowRoot?.querySelector('ntp-searchbox')?.shadowRoot?.querySelector('cr-searchbox-input')?.shadowRoot?.querySelector('input')");
        await JS("window.app=document.querySelector('ntp-app').shadowRoot;window.box=app.querySelector('ntp-searchbox');window.input=box.getInputElement().inputElement;window.tiles=app.querySelector('cr-most-visited');window.mv=tiles.shadowRoot;window.matches=()=>box.getDropdownElement().shadowRoot.querySelectorAll('cr-searchbox-match');window.rect=e=>e.getBoundingClientRect();");
        await Until("!!mv.querySelector('#addShortcut')");
    }
    private static async Task Focus(){await JS("input.focus();input.dispatchEvent(new MouseEvent('mousedown',{button:0,bubbles:true,composed:true}))");}
    private static async Task Type(string text){
        await JS("input.focus();input.select()");
        await view.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.insertText",JsonConvert.SerializeObject(new{text}));
    }
    private static async Task Key(string key){
        int code=key=="Escape"?27:key=="ArrowDown"?40:key=="ArrowUp"?38:key=="PageDown"?34:key=="PageUp"?33:key=="Enter"?13:9;
        await view.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchKeyEvent",JsonConvert.SerializeObject(new{type="keyDown",key,windowsVirtualKeyCode=code}));
        await view.CoreWebView2.CallDevToolsProtocolMethodAsync("Input.dispatchKeyEvent",JsonConvert.SerializeObject(new{type="keyUp",key,windowsVirtualKeyCode=code}));
        await Task.Delay(25);
    }
    private static async Task<List<string>> Suggest(string query,string engine,CancellationToken token){
        requestStarts.Add(clock.ElapsedMilliseconds);
        if(online&&query.StartsWith("weather in perth"))return await SearchSuggestions.GetForEngineAsync(query,engine,token);
        await Task.Delay(query.Contains("slow")?350:45,token);
        return new List<string>{query+" alpha",query+" beta",query+" gamma"};
    }
    private static async Task Add(string name,string url){
        await JS("mv.querySelector('#addShortcut').click()");await Task.Delay(40);
        await JS("mv.querySelector('#dialogInputName').value="+JsonConvert.SerializeObject(name)+";mv.querySelector('#dialogInputUrl').value="+JsonConvert.SerializeObject(url));await Task.Delay(40);
        await JS("mv.querySelector('.action-button').click()");await Task.Delay(80);
    }
    [STAThread]
    private static void Main(string[] args){
        output=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"newtab-chromium-render");Directory.CreateDirectory(output);
        bool preview = File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"preview-mode"));
        online=preview||args.Contains("--online");
        Check(NewTabPageData.RecentHistory(history,profile,"",false).Count==2,"history profile isolation");
        Check(NewTabPageData.RecentHistory(history,profile,"",true).Count==0,"private history isolation");
        Check(!NewTabPageData.IsPage("https://quartz.com:444/newtab/index.html")&&!NewTabPageData.IsPage("https://quartz.com.evil.test/newtab/index.html"),"bridge origin validation");
        Check(NewTabPageData.SearchUrl("a & b","google")=="https://www.google.com/search?q=a%20%26%20b","query encoding");
        var fixture=Path.Combine(output,"history-"+Guid.NewGuid().ToString("N")+".json");
        var other=Guid.NewGuid();
        File.WriteAllText(fixture,JsonConvert.SerializeObject(new[]{
            new HistoryModel{ProfileId=profile,WebAddress="https://example.org/recent"},
            new HistoryModel{ProfileId=profile,WebAddress="https://example.org/recent"},
            new HistoryModel{ProfileId=other,WebAddress="https://example.org/recent"},
            new HistoryModel{ProfileId=profile,WebAddress="https://example.org/keep"}}));
        new HistoryService(fixture).DeleteProfileUrl(profile,"https://example.org/recent");
        var remaining=JsonConvert.DeserializeObject<List<HistoryModel>>(File.ReadAllText(fixture));
        Check(remaining.Count==2&&remaining.Any(h=>h.ProfileId==other)&&remaining.Any(h=>h.WebAddress.EndsWith("/keep")),"history deletion persists all matching visits and preserves other profiles/URLs");
        Check(JsonConvert.SerializeObject(NewTabPageIcons.Monogram("https://www.example.co.uk/" )).Contains("\"text\":\"E\""),"Chromium registrable-domain monogram");
        Check(JsonConvert.SerializeObject(NewTabPageIcons.Monogram("https://127.0.0.1/" )).Contains("\"text\":\"IP\""),"Chromium IP monogram");
        Application.EnableVisualStyles();
        var form=new Form{ClientSize=new Size(1200,800),StartPosition=FormStartPosition.Manual,Location=preview?new Point(80,80):new Point(-2400,-2400),ShowInTaskbar=preview,Text="Quartz new-tab verification"};
        view=new WebView2{Dock=DockStyle.Fill};form.Controls.Add(view);
        form.Shown+=async(s,e)=>{
            NewTabPageController controller=null;
            try{
                var env=await CoreWebView2Environment.CreateAsync(null,Path.Combine(output,"profile-"+Guid.NewGuid().ToString("N")));
                await view.EnsureCoreWebView2Async(env);var core=view.CoreWebView2;
                core.SetVirtualHostNameToFolderMapping("quartz.com",Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"assets","quartz.com"),CoreWebView2HostResourceAccessKind.DenyCors);
                await core.AddScriptToExecuteOnDocumentCreatedAsync("window.__errors=[];addEventListener('error',e=>__errors.push(e.message));addEventListener('unhandledrejection',e=>__errors.push(String(e.reason)))");
                controller=new NewTabPageController(core,profile,false,key=>key=="Theme"?theme:"google",()=>history,url=>history.RemoveAll(h=>h.ProfileId==profile&&h.WebAddress==url),Suggest);
                core.NavigationStarting+=(sender,nav)=>{if(NewTabPageData.IsPage(core.Source)&&!NewTabPageData.IsPage(nav.Uri)){destination=nav.Uri;nav.Cancel=true;}};
                core.Navigate(NewTabPageData.PageUrl);await Bind();await Task.Delay(200);
                if(preview){form.FormClosed+=(a,b)=>controller.Dispose();return;}
                await Assert("__errors.length===0","Chromium components initialize without script errors");
                await Assert("app.querySelector('quartz-logo').shadowRoot.querySelector('img').naturalWidth>0","Quartz logo loads");
                await Assert("getComputedStyle(input).fontFamily.includes('Segoe UI')&&getComputedStyle(input).fontSize==='16px'","original Windows Chromium typography");
                await Assert("Math.round(rect(box).height)===48&&Math.round(rect(box).width)===561","original searchbox dimensions");
                await Assert("document.documentElement.scrollHeight<=innerHeight","no unwanted vertical scrollbar");
                await Capture("light");
                await Focus();await Until("matches().length===2");
                await Assert("matches()[0].match.contents==='Recent Quartz page'&&!box.result.matches.some(m=>m.contents==='OTHER PROFILE')","empty click shows newest Quartz history");
                await Assert("Array.from(matches()).every(m=>m.match.supportsDeletion&&!m.shadowRoot.querySelector('#remove').hidden)","all local history rows have Chromium's remove control");
                await Assert("(()=>{const a=rect(box.getInputElement().shadowRoot.querySelector('cr-searchbox-icon'));const b=rect(matches()[0].shadowRoot.querySelector('cr-searchbox-icon'));return Math.abs((a.x+a.width/2)-(b.x+b.width/2))<1})()","input and suggestion icon columns align");
                await Assert("(()=>{const row=matches()[0];const r=rect(row);const i=rect(row.shadowRoot.querySelector('cr-searchbox-icon'));return Math.round(r.height)===44&&Math.abs(i.y+i.height/2-r.y-r.height/2)<1})()","original 44px suggestion rows and centered icons");
                await Capture("history");
                await JS("matches()[0].shadowRoot.querySelector('#remove').click()");await Until("matches().length===1");
                Check(history.All(h=>h.WebAddress!="https://example.org/recent"),"history X removes the backing entry");
                await Key("Escape");await Assert("input.value===''&&!box.dropdownIsVisible","Escape clears an unselected zero-prefix list");
                await Type("speed");
                await Assert("box.result.matches[0].contents==='speed'&&box.dropdownIsVisible","verbatim row updates before network results");
                await Until("box.result.matches.some(m=>m.contents==='speed alpha')");
                await JS("window.retained=matches()[1];input.value='speed slow';input.dispatchEvent(new InputEvent('input',{bubbles:true,composed:true}))");
                await Assert("box.result.matches[0].contents==='speed slow'&&matches()[1]===retained&&box.dropdownIsVisible","typing updates the first row without clearing/replacing existing suggestion elements");
                await Key("ArrowDown");await Task.Delay(450);
                await Assert("box.selectedMatchIndex===1&&input.value==='speed alpha'","late responses do not override keyboard selection");
                await Key("Escape");await Assert("box.selectedMatchIndex===0&&box.dropdownIsVisible&&input.value==='speed slow'","Escape from a lower result selects the first result");
                await Key("Escape");await Assert("input.value===''&&!box.dropdownIsVisible","second Escape clears input and results");
                await Type("old slow");await Type("new");await Until("box.result.matches.some(m=>m.contents==='new alpha')");await Task.Delay(400);
                await Assert("box.result.input==='new'&&!box.result.matches.some(m=>m.contents==='old slow alpha')","out-of-order network responses are discarded");
                await Type("close slow");await Key("Escape");await Task.Delay(450);
                await Assert("input.value===''&&!box.dropdownIsVisible","Escape prevents late requests from reopening results");
                requestStarts.Clear();await Type("pacea");await Task.Delay(20);await Type("paceb");await Task.Delay(20);await Type("pacec");await Task.Delay(20);await Type("paced");await Task.Delay(250);
                Check(requestStarts.Count>=2&&requestStarts.Zip(requestStarts.Skip(1),(a,b)=>b-a).All(ms=>ms>=90),"network requests are spaced from request time during continuous typing");
                await Type("fast enter");await Key("Enter");Check(destination=="https://www.google.com/search?q=fast%20enter","fast Enter navigates the current query");
                await Type("pages");await Until("matches().length===4");await Key("PageDown");await Assert("box.selectedMatchIndex===3","PageDown selects last result");await Key("PageUp");await Assert("box.selectedMatchIndex===0","PageUp selects first result");
                await Capture("suggestions");
                if(online){
                    await Type("weather in perth");await Until("box.result.input==='weather in perth'&&box.result.matches.some(m=>m.type==='search-suggest'&&!m.contents.StartsWith)",8000);
                    await Until("box.result.matches.some(m=>m.type==='search-suggest'&&m.contents.toLowerCase().includes('perth')&&!m.contents.endsWith(' alpha'))",8000);
                    await Capture("live-google");
                    await Assert("box.result.matches.filter(m=>m.type==='search-suggest').length>0","live Google suggestions arrive through the native bridge");
                }
                await Key("Escape");await Add("Chromium","https://www.chromium.org/");await Add("Example","https://example.org/");
                await Assert("mv.querySelectorAll('.tile').length===2","native add-shortcut flow");
                await Until("Array.from(mv.querySelectorAll('.tile-icon img')).every(i=>i.complete&&i.naturalWidth>1)");
                await Assert("Array.from(mv.querySelectorAll('.tile-icon img')).every(i=>Math.round(rect(i).width)===24)&&Math.round(rect(mv.querySelector('.tile-icon')).width)===48","24px shortcut favicons inside original 48px circles");
                await Task.Delay(350);
                await Assert("(()=>{const r=rect(mv.querySelector('#toastManager').shadowRoot.querySelector('cr-toast'));return Math.abs(r.left-24)<1&&Math.abs(innerHeight-r.bottom-24)<1})()","toast is anchored 24px from the lower-left corner");
                await Capture("shortcut-toast");
                await JS("mv.querySelector('.tile cr-icon-button').click()");await Task.Delay(40);await Capture("shortcut-menu");
                await JS("mv.querySelector('#actionMenuViewOrEdit').click()");await Task.Delay(50);await Capture("edit-shortcut");
                await Assert("mv.querySelector('#dialog').open","native edit dialog opens");
                await JS("mv.querySelector('#dialogInputName').value='<b>Literal title</b>'");await Task.Delay(40);await JS("mv.querySelector('.action-button').click()");await Task.Delay(80);
                await Assert("mv.querySelector('.tile-title').textContent.trim()==='<b>Literal title</b>'&&!mv.querySelector('.tile-title b')","shortcut titles are rendered as text");
                await JS("mv.querySelector('.tile cr-icon-button').click();mv.querySelector('#actionMenuRemove').click()");await Task.Delay(80);
                await Assert("mv.querySelectorAll('.tile').length===1","native shortcut remove flow");await JS("mv.querySelector('#undo').click()");await Task.Delay(80);
                await Assert("mv.querySelectorAll('.tile').length===2","native undo restores shortcut");
                await JS("mv.querySelector('#toastManager').hide();document.activeElement?.blur()");
                await Assert("getComputedStyle(mv.querySelector('.tile')).transitionDuration==='0.3s'","original 300ms shortcut transition");
                await JS("(()=>{const a=mv.querySelectorAll('.tile')[0],b=mv.querySelectorAll('.tile')[1];window.ra=rect(a);window.rb=rect(b);a.querySelector('a').dispatchEvent(new DragEvent('dragstart',{bubbles:true,composed:true,clientX:ra.x+30,clientY:ra.y+30,dataTransfer:new DataTransfer()}));document.dispatchEvent(new DragEvent('dragover',{bubbles:true,clientX:rb.x+30,clientY:rb.y+30,dataTransfer:new DataTransfer()}))})()");await Task.Delay(120);
                await Assert("tiles.hasAttribute('reordering_')&&!!mv.querySelector('.dragging')","native drag interaction starts");await Capture("drag-intermediate");
                await JS("document.dispatchEvent(new DragEvent('drop',{bubbles:true,clientX:rb.x+30,clientY:rb.y+30,dataTransfer:new DataTransfer()}));document.dispatchEvent(new DragEvent('dragend',{bubbles:true}));tiles.dispatchEvent(new PointerEvent('pointermove',{bubbles:true}))");await Task.Delay(100);
                await Assert("mv.querySelector('.tile-title').textContent.trim()==='Example'","native drag reorders shortcuts");
                core.Reload();await Task.Delay(200);await Bind();await Until("mv.querySelectorAll('.tile').length===2");
                await Assert("mv.querySelector('.tile-title').textContent.trim()==='Example'","shortcut order persists across reload");
                foreach(var name in new[]{"dark","black","aqua","xmas"}){theme=name;controller.UpdateTheme();await Until("document.documentElement.dataset.theme==='"+name+"'");await Capture(name);}
                foreach(var width in new[]{360,600,800}){form.ClientSize=new Size(width,640);await Task.Delay(100);await Assert("Math.round(rect(box).width)===(innerWidth>=672?561:innerWidth>=560?449:337)&&document.documentElement.scrollWidth<=innerWidth","original responsive widths at "+width);}
                await Assert("__errors.length===0","no runtime errors after interactions");
                Console.WriteLine("All "+checks+" checks passed. Renders: "+output);
            }catch(Exception ex){Console.Error.WriteLine(ex);if(view.CoreWebView2!=null){Console.Error.WriteLine(await JS("JSON.stringify(window.__errors)"));await Capture("failure");}Environment.ExitCode=1;}
            finally{if(!preview){controller?.Dispose();form.Close();}}
        };
        Application.Run(form);
    }
}

