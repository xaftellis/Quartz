// Browser-process adapter for the unmodified Chromium SearchboxMixin.
import {handler,callbackRouter,hostEvents,send,createAutocompleteMatch} from './host';
const icons='/newtab/chromium/cr_components/searchbox/icons/';
let engine='google',engineName='Google',request=null,current=null,sequence=0;
let previousHistory=[],previousSearches=[];
const deleted=new Set();
const engineUrls={google:'https://www.google.com/search?q=',bing:'https://www.bing.com/search?q=',duckduckgo:'https://duckduckgo.com/?q=',yahoo:'https://search.yahoo.com/search?p=',youtube:'https://www.youtube.com/results?search_query=',wikipedia:'https://wikipedia.org/w/index.php?search=',netflix:'https://www.netflix.com/search?q=',googlemaps:'https://www.google.com/maps/search/',ebay:'https://www.ebay.com/sch/?_nkw=',amazon:'https://www.amazon.com/s?k=',amazom:'https://www.amazon.com/s?k=',ecosia:'https://www.ecosia.org/search?q='};
function searchUrl(text){return (engineUrls[engine]||engineUrls.google)+encodeURIComponent(text);}
function navigationUrl(text){
  try{
    if(/\s/.test(text))return null;
    const u=new URL(text.includes('://')?text:`https://${text}`);
    return ['https:','http:'].includes(u.protocol)&&!u.username&&!u.password&&
      (text.includes('://')||u.hostname.includes('.')||u.hostname==='localhost'||u.hostname.startsWith('['))?u.href:null;
  }catch{return null;}
}
function classify(text,query,search){
  const i=text.toLocaleLowerCase().indexOf(query.toLocaleLowerCase());
  if(!query||i<0)return [{offset:0,style:query&&search?2:0}];
  const styles=[];
  if(i)styles.push({offset:0,style:search?2:0});
  styles.push({offset:i,style:search?0:2});
  if(i+query.length<text.length)styles.push({offset:i+query.length,style:search?2:0});
  return styles;
}
function historicalSearch(url){
  try{
    const u=new URL(url);
    if(/(^|\.)google\.[a-z.]+$/.test(u.hostname)&&u.pathname==='/search')return u.searchParams.get('q');
    if(u.hostname==='www.bing.com'&&u.pathname==='/search')return u.searchParams.get('q');
    if(u.hostname==='duckduckgo.com')return u.searchParams.get('q');
    if(u.hostname==='search.yahoo.com'&&u.pathname==='/search')return u.searchParams.get('p');
    if(u.hostname==='www.youtube.com'&&u.pathname==='/results')return u.searchParams.get('search_query');
  }catch{}
  return null;
}
function historyMatches(query){
  return previousHistory.filter(h=>!deleted.has(h.url)&&(!query||`${h.text} ${h.url}`.toLocaleLowerCase().includes(query.toLocaleLowerCase())))
    .slice(0,query?3:8).map(h=>{
      const term=historicalSearch(h.url),text=term||h.text;
      return createAutocompleteMatch({contents:text,contentsClass:classify(text,query,!!term),
        description:term?`${engineName} Search`:h.url.replace(/^https?:\/\//,''),
        descriptionClass:[{offset:0,style:term?4:1}],a11yLabel:`${text}, ${h.url}`,
        fillIntoEdit:term||h.url,destinationUrl:h.url,isSearchType:!!term,
        type:term?'search-history':'history-title',iconPath:icons+(term?'history_cr23.svg':'page_cr23.svg'),
        supportsDeletion:true,removeButtonA11yLabel:`Remove ${text} from history`,historyUrl:h.url});
    });
}
function publish(){
  if(!request)return;
  const query=request.input.trim(),matches=[];
  if(query){
    const url=navigationUrl(query);
    matches.push(createAutocompleteMatch({contents:query,fillIntoEdit:query,
      destinationUrl:url||searchUrl(query),a11yLabel:`${query}${url?'':`, ${engineName} Search`}`,
      isSearchType:!url,allowedToBeDefaultMatch:true,type:url?'url-what-you-typed':'search-what-you-typed',
      iconPath:icons+(url?'page_cr23.svg':'search_cr23.svg'),description:url?'':`${engineName} Search`,descriptionClass:[{offset:0,style:4}]}));
  }
  matches.push(...historyMatches(query));
  const seen=new Set(matches.map(m=>m.contents.toLocaleLowerCase()));
  if(query)for(const text of previousSearches){
    if(seen.has(text.toLocaleLowerCase()))continue;seen.add(text.toLocaleLowerCase());
    matches.push(createAutocompleteMatch({contents:text,contentsClass:classify(text,query,true),
      a11yLabel:`${text}, search`,fillIntoEdit:text,destinationUrl:searchUrl(text),
      isSearchType:true,type:'search-suggest',iconPath:icons+'search_cr23.svg'}));
  }
  current={input:request.input,matches:matches.slice(0,8),suggestionGroupsMap:{},queryId:request.queryId,sequenceId:++sequence};
  callbackRouter.autocompleteResultChanged.emit(current);
}
function renderedMatch(index,url){
  const box=document.querySelector('ntp-app')?.shadowRoot?.querySelector('ntp-searchbox');
  const match=box?.result?.matches[index];
  return match&&(!url||match.destinationUrl===url)?match:null;
}
Object.assign(handler,{
  queryAutocomplete(queryId,tabId,input,preventInlineAutocomplete){
    request={queryId,input,preventInlineAutocomplete,id:0};
    // Verbatim and cached local matches are delivered in the same input event.
    publish();
    request.id=send('suggest',{query:input.trim()});
  },
  stopAutocomplete(clearResult){
    request=null;send('stop-suggest');
    if(clearResult){current=null;previousSearches=[];}
  },
  openAutocompleteMatch(index,url,showing,button,modifiers){
    const match=renderedMatch(index,url);if(!match)return;
    if(button===1||modifiers?.ctrlKey||modifiers?.metaKey||modifiers?.shiftKey)
      window.open(match.destinationUrl,'_blank');
    else send('navigate',{text:match.historyUrl||match.fillIntoEdit,forceSearch:match.type==='search-suggest'});
  },
  deleteAutocompleteMatch(index,url){
    const match=renderedMatch(index,url);if(!match?.supportsDeletion)return;
    deleted.add(match.historyUrl);send('delete-history',{url:match.historyUrl});
    // Chromium publishes a post-deletion result without moving focus to the X.
    if(!request&&current)request={queryId:current.queryId,input:current.input,id:0};
    publish();
  },
  onFocusChanged(){},onNavigationLikely(){},onPopupSelectionChanged(){},
});
hostEvents.addEventListener('state',({detail})=>{
  engine=detail.engine||'google';
  engineName=({google:'Google',bing:'Bing',duckduckgo:'DuckDuckGo',yahoo:'Yahoo',youtube:'YouTube'})[engine]||engine;
});
hostEvents.addEventListener('suggestions',({detail})=>{
  if(!request||detail.id!==request.id||detail.query!==request.input.trim())return;
  previousHistory=detail.history||[];
  if(detail.complete)previousSearches=detail.searches||[];
  publish();
});
hostEvents.addEventListener('history-deleted',({detail})=>{
  if(detail.success){previousHistory=previousHistory.filter(h=>h.url!==detail.url);}
  else deleted.delete(detail.url);
  if(request)request.id=send('suggest',{query:request.input.trim()});
});

