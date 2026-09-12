// Chromium owns input, keyboard, selection, focus, deletion and Escape behavior.
import {CrLitElement,html} from '//resources/lit/v3_0/lit.rollup.js';
import {SearchboxMixin} from '//resources/cr_components/searchbox/searchbox_mixin.js';
import {getCss} from 'chrome://new-tab-page/ntp_searchbox.css.js';
import {callbackRouter,hostEvents,handler} from './host';
import './autocomplete';

export class QuartzSearchbox extends SearchboxMixin(CrLitElement) {
  static get properties(){return {placeholder:{type:String},engine:{type:String}};}
  static get styles(){return getCss();}
  accessor placeholder='Search Google or type a URL';
  accessor engine='Google';
  listenerId;
  refreshIcons=()=>{
    this.$.input?.shadowRoot?.querySelector('cr-searchbox-icon')?.requestUpdate('match');
    for(const match of this.$.matches?.shadowRoot?.querySelectorAll('cr-searchbox-match')||[])
      match.shadowRoot?.querySelector('cr-searchbox-icon')?.requestUpdate('match');
  };
  constructor(){
    super();
    this.searchboxIcon='/newtab/chromium/cr_components/searchbox/icons/search_cr23.svg';
    this.searchboxAriaDescription='Search or type a URL';
  }
  connectedCallback(){
    super.connectedCallback();
    this.listenerId=callbackRouter.autocompleteResultChanged.addListener(this.onAutocompleteResultChanged.bind(this));
    hostEvents.addEventListener('icons-updated',this.refreshIcons);
  }
  disconnectedCallback(){
    callbackRouter.removeListener(this.listenerId);
    hostEvents.removeEventListener('icons-updated',this.refreshIcons);
    super.disconnectedCallback();
  }
  pageHandler(){return handler;}
  getInputElement(){return this.$.input;}
  getDropdownElement(){return this.$.matches;}
  getWrapperElement(){return this.$.inputWrapper;}
  render(){return html`
    <div id="inputWrapper" @focusout=${this.onInputWrapperFocusout} @keydown=${this.onInputWrapperKeydown}>
      <cr-searchbox-input id="input" exportparts="searchbox-input"
          .dropdownIsVisible=${this.dropdownIsVisible} .inputAriaLive=${this.inputAriaLive}
          .placeholderText=${this.placeholder} .searchboxAriaDescription=${this.searchboxAriaDescription}
          .searchboxIcon=${this.searchboxIcon} .selectedMatch=${this.selectedMatch}
          .inputKeywordModel=${this.inputKeywordModel} .inputHasMatches=${this.hasMatches()}
          @searchbox-input-text-updated=${this.onSearchboxInputTextUpdated}
          @input-focus-changed=${this.onInputFocusChanged}>
      </cr-searchbox-input>
      <div class="dropdownContainer">
        <cr-searchbox-dropdown id="matches" part="searchbox-dropdown" exportparts="dropdown-content"
            role="listbox" .result=${this.result} .selection=${this.selection}
            .selectedMatchIndex=${this.selectedMatchIndex}
            @selected-match-index-changed=${this.onSelectedMatchIndexChanged}
            @match-focusin=${this.onMatchFocusin} @match-click=${this.onMatchClick}
            ?hidden=${!this.dropdownIsVisible}>
        </cr-searchbox-dropdown>
      </div>
    </div>`;}
}
customElements.define('ntp-searchbox',QuartzSearchbox);

