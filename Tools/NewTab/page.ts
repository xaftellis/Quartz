import {CrLitElement,html,css} from '//resources/lit/v3_0/lit.rollup.js';
import {getCss} from 'chrome://new-tab-page/app.css.js';
import {getCss as getLogoCss} from 'chrome://new-tab-page/logo.css.js';
import {hostEvents,send} from './host';
const engines={google:'Google',bing:'Bing',duckduckgo:'DuckDuckGo',yahoo:'Yahoo',youtube:'YouTube',wikipedia:'Wikipedia',netflix:'Netflix',googlemaps:'Google Maps',ebay:'eBay',amazon:'Amazon',amazom:'Amazon',ecosia:'Ecosia'};
const palettes={light:{tile:0xffe5e7e8,dark:false},dark:{tile:0xff454545,dark:true},black:{tile:0xff242424,dark:true},aqua:{tile:0xffc1f0f4,dark:false},xmas:{tile:0xffa30000,dark:true}};
class QuartzLogo extends CrLitElement {
  static get styles(){return [getLogoCss(),css`img {width:144px;height:144px;object-fit:contain;}`];}
  render(){return html`<img src="quartz.png" alt="Quartz" width="144" height="144" draggable="false">`;}
}
customElements.define('quartz-logo',QuartzLogo);
class QuartzNewTab extends CrLitElement {
  static get styles(){return getCss();}
  handleState=({detail})=>{
    this.theme(detail.theme);
    this.$.searchbox.engine=engines[detail.engine]||'Google';
    this.$.searchbox.placeholder=`Search ${this.$.searchbox.engine} or type a URL`;
  };
  refreshIcons=()=>this.$.mostVisited.requestUpdate();
  async connectedCallback(){
    super.connectedCallback();
    await this.updateComplete;
    this.theme(document.documentElement.dataset.theme||'light');
    hostEvents.addEventListener('state',this.handleState);
    hostEvents.addEventListener('icons-updated',this.refreshIcons);
    send('state');
  }
  disconnectedCallback(){
    hostEvents.removeEventListener('state',this.handleState);
    hostEvents.removeEventListener('icons-updated',this.refreshIcons);
    super.disconnectedCallback();
  }
  theme(name){
    const palette=palettes[name]||palettes.light;
    document.documentElement.dataset.theme=name in palettes?name:'light';
    this.$.mostVisited.theme={backgroundColor:{value:palette.tile},isDark:palette.dark,useWhiteTileIcon:palette.dark};
    this.$.searchbox.toggleAttribute('is-dark',palette.dark);
  }
  render(){return html`
    <div id="content">
      <quartz-logo id="logo"></quartz-logo>
      <div id="searchboxContainer"><ntp-searchbox id="searchbox" shown></ntp-searchbox></div>
      <cr-most-visited id="mostVisited" single-row reflow-on-overflow></cr-most-visited>
    </div>`;}
}
customElements.define('ntp-app',QuartzNewTab);
document.addEventListener('visibilitychange',()=>{if(!document.hidden)send('state');});
