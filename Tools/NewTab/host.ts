import {drawMonogram} from './monogram';
// Quartz implementations of Chromium's browser-process APIs. UI components
// and animations are imported from the pinned Chromium source unchanged.
export const loadTimeData = {
  data: {
    addLinkTitle:'Add shortcut', editLinkTitle:'Edit shortcut', nameField:'Name', urlField:'URL',
    linkCancel:'Cancel', linkDone:'Done', linkRemove:'Remove', linkRemoveA11y:'Remove $1',
    shortcutMoreActions:'More actions for $1', shortcutAlreadyExists:'Shortcut already exists',
    invalidUrl:'Enter a valid URL', linkAddedMsg:'Shortcut added', linkEditedMsg:'Shortcut edited',
    linkRemovedMsg:'Shortcut removed', linkCantCreate:'Can’t create shortcut', linkCantEdit:'Can’t edit shortcut',
    undo:'Undo', undoDescription:'Undo last action', restoreDefaultLinks:'Restore default shortcuts',
    restoreThumbnailsShort:'Restore all', showMore:'Show more', showLess:'Show less',
    searchBoxHint:'Search Google or type a URL', searchboxSeparator:' – ', removeSuggestion:'Remove suggestion',
    close:'Close', invalid:'Invalid', isWindows:true, realboxVirtualFocusNavigation:false,
    mostVisitedHighDpiFaviconsEnabled:true, reportMetrics:false,
  },
  isInitialized() { return true; },
  getBoolean(name) { return !!this.data[name]; },
  getInteger(name) { return this.data[name] ?? 0; },
  getString(name) { return this.data[name] ?? name; },
  getStringF(name, ...args) { return this.getString(name).replace(/\$(\d)/g, (_,n) => args[+n-1] ?? ''); },
  valueExists(name) { return name in this.data; },
  getValue(name) { return this.data[name]; },
  overrideValues(data) { Object.assign(this.data,data); }
};
export const TileSource = {TOP_SITES:0, POPULAR:1, POPULAR_BAKED_IN:2, CUSTOM_LINKS:3, ALLOWLIST:4, HOMEPAGE:5, ENTERPRISE_SHORTCUTS:6};
export const TextDirection = {UNKNOWN_DIRECTION:0, RIGHT_TO_LEFT:1, LEFT_TO_RIGHT:2};
export const KeywordType = {kChip:0,kInKeyword:1,kInstant:2};
export const SideType = {kDefaultPrimary:0,kSecondary:1};
export const RenderType = {kDefaultVertical:0,kHorizontal:1,kGrid:2};
export const SelectionLineState = {kNormal:1,kKeywordMode:2,kFocusedButtonAction:3,kFocusedButtonRemoveSuggestion:4,kFocusedButtonAim:5,kFocusedButtonContextEntrypoint:6,kCtrlEnter:7};
export const SelectionDirection = {kForward:1,kBackward:2};
export const SelectionStep = {kWholeLine:1,kStateOrLine:2,kAllLines:3};
export const NavigationPredictor = {kMouseDown:0,kMouseOver:1,kTouchDown:2,kUpOrDownArrowButton:3};
export const SuggestInventory = {kDefault:0};
export const InputMethod = {kKeyboard:0};
export const MetricsReporterImpl = {getInstance: () => ({hasLocalMark:()=>false,mark(){},clearMark(){},measure:async()=>0,reportTime(){}})};

let listenerId = 0;
class Signal {
  listeners = new Map();
  addListener(fn) { const id=++listenerId; this.listeners.set(id,fn); return id; }
  emit(...args) { for (const fn of this.listeners.values()) fn(...args); }
}
export const callbackRouter = {
  setMostVisitedInfo:new Signal(), onMostVisitedTilesAutoRemoval:new Signal(), setInputText:new Signal(),
  autocompleteResultChanged:new Signal(), setKeywordSpaceTriggeringEnabled:new Signal(), setAvailableKeywordModels:new Signal(),
  removeListener(id) { for (const value of Object.values(this)) if (value instanceof Signal) value.listeners.delete(id); }
};
export const hostEvents = new EventTarget();
let messageId = 0;
export function send(type, data = {}) {
  const id=++messageId;
  window.chrome?.webview?.postMessage({channel:'quartz-newtab',id,type,...data});
  return id;
}
window.chrome?.webview?.addEventListener('message',({data})=>hostEvents.dispatchEvent(new CustomEvent(data.type,{detail:data})));

const storageKey = 'quartz.newtab.shortcuts.v1';
let undo = null;
export function normalizeUrl(value) {
  try {
    const url=new URL(value.includes('://')?value:`https://${value}/`);
    return ['http:','https:'].includes(url.protocol) && !url.username && !url.password ? url.href : null;
  } catch { return null; }
}
function saved() {
  const data=JSON.parse(localStorage.getItem(storageKey)||'[]');
  if (!Array.isArray(data) || data.length>10 || data.some(s=>!s || typeof s.id!=='string' || typeof s.name!=='string' || typeof s.url!=='string' || !normalizeUrl(s.url))) throw Error('Invalid shortcuts');
  return data;
}
function publish() {
  let items;
  try { items=saved(); } catch { items=[]; hostEvents.dispatchEvent(new CustomEvent('storage-error')); }
  callbackRouter.setMostVisitedInfo.emit({visible:true,customLinksEnabled:true,enterpriseShortcutsEnabled:false,
    tiles:items.map(s=>({id:s.id,url:s.url,title:s.name,titleDirection:TextDirection.LEFT_TO_RIGHT,
      isQueryTile:false,allowUserEdit:true,allowUserDelete:true,source:TileSource.CUSTOM_LINKS,titleSource:0}))});
  if (items.length) send('icons',{urls:items.map(s=>s.url)});
}
function change(edit) {
  try {
    const before=saved(), items=before.map(s=>({...s}));
    if (edit(items)===false) return {success:false};
    const after=JSON.stringify(items);
    localStorage.setItem(storageKey,after);
    undo={before:JSON.stringify(before),after}; publish(); return {success:true};
  } catch { hostEvents.dispatchEvent(new CustomEvent('storage-error')); return {success:false}; }
}
function navigate(url,button=0,ctrl=false,shift=false) {
  if (button===1 || ctrl || shift) window.open(url,'_blank');
  else send('navigate',{text:url});
}
export const handler = {
  getMostVisitedExpandedState:async()=>({isExpanded:false}), setMostVisitedExpandedState(){},
  updateMostVisitedInfo:publish,
  addMostVisitedTile:async(url,title)=>change(items=>{
    url=normalizeUrl(url); if (!url || items.length>=10 || items.some(s=>normalizeUrl(s.url)===url)) return false;
    items.push({id:crypto.randomUUID(),name:title,url});
  }),
  updateMostVisitedTile:async(tile,url,title)=>change(items=>{
    const item=items.find(s=>s.id===tile.id); url=normalizeUrl(url);
    if (!item || !url || items.some(s=>s.id!==item.id && normalizeUrl(s.url)===url)) return false;
    item.name=title; item.url=url;
  }),
  deleteMostVisitedTile:tile=>change(items=>{const i=items.findIndex(s=>s.id===tile.id); if(i<0)return false;items.splice(i,1);}),
  reorderMostVisitedTile:(tile,index)=>change(items=>{const i=items.findIndex(s=>s.id===tile.id);if(i<0)return false;items.splice(index,0,items.splice(i,1)[0]);}),
  undoMostVisitedTileAction(){
    try {if(undo && localStorage.getItem(storageKey)===undo.after){localStorage.setItem(storageKey,undo.before);undo=null;publish();}}
    catch {hostEvents.dispatchEvent(new CustomEvent('storage-error'));}
  },
  restoreMostVisitedDefaults:()=>change(items=>{items.length=0;}),
  onMostVisitedTileNavigation:(tile,index,button,alt,ctrl,meta,shift)=>navigate(tile.url,button,ctrl||meta,shift),
  onMostVisitedTilesRendered(){}, preconnectMostVisitedTile(){}, prefetchMostVisitedTile(){}, prerenderMostVisitedTile(){}, cancelPrerender(){},
  openAutocompleteMatch(index,url,showing,button,modifiers){hostEvents.dispatchEvent(new CustomEvent('open-match',{detail:{index,button,modifiers}}));},
  onNavigationLikely(){}, deleteAutocompleteMatch(index){hostEvents.dispatchEvent(new CustomEvent('remove-match',{detail:{index}}));},
};
export const browserProxyFactory = {getInstance:()=>({handler,callbackRouter})};
export const SearchboxBrowserProxy = browserProxyFactory;
window.addEventListener('storage',e=>{if(e.key===storageKey || e.key===null)publish();});

const favicons = new Map();
const monograms = new Map();
const emptyIcon='data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=';
export function faviconUrl(url) { return favicons.get(url) || monograms.get(url) || emptyIcon; }
export function getFaviconUrl(url) { return favicons.get(url) || ''; }
hostEvents.addEventListener('icons',({detail})=>{
  for(const [url,image] of Object.entries(detail.icons||{}))
    if(typeof image==='string' && image.startsWith('data:image/x-icon;base64,'))favicons.set(url,image);
  for(const [url,data] of Object.entries(detail.fallbacks||{}))monograms.set(url,drawMonogram(data));
  hostEvents.dispatchEvent(new CustomEvent('icons-updated'));
});
// AutocompleteMatch defaults from Chromium's searchbox_browser_proxy.ts.
export function createAutocompleteMatch(modifiers={}) {
  return Object.assign({isHidden:false,a11yLabel:'',actions:[],allowedToBeDefaultMatch:false,isSearchType:false,
    isEnterpriseSearchAggregatorPeopleType:false,swapContentsAndDescription:false,showContextualDescription:false,
    supportsDeletion:false,suggestionGroupId:-1,contents:'',contentsClass:[{offset:0,style:0}],description:'',descriptionClass:[{offset:0,style:0}],
    destinationUrl:'',inlineAutocompletion:'',fillIntoEdit:'',iconPath:'',iconUrl:'',imageDominantColor:'',imageUrl:'',
    isContextualSuggestion:false,isNoncannedAimSuggestion:false,removeButtonA11yLabel:'',type:'',isTwoRowSuggestion:false,
    tailSuggestCommonPrefix:null,keywordModel:null,fuseboxAction:null,suggestStyle:0},modifiers);
}



