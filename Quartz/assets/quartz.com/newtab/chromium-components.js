// Tools/NewTab/node_modules/.pnpm/@lit+reactive-element@2.1.2/node_modules/@lit/reactive-element/css-tag.js
var t = globalThis;
var e = t.ShadowRoot && (void 0 === t.ShadyCSS || t.ShadyCSS.nativeShadow) && "adoptedStyleSheets" in Document.prototype && "replace" in CSSStyleSheet.prototype;
var s = Symbol();
var o = /* @__PURE__ */ new WeakMap();
var n = class {
  constructor(t5, e5, o5) {
    if (this._$cssResult$ = true, o5 !== s) throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");
    this.cssText = t5, this.t = e5;
  }
  get styleSheet() {
    let t5 = this.o;
    const s5 = this.t;
    if (e && void 0 === t5) {
      const e5 = void 0 !== s5 && 1 === s5.length;
      e5 && (t5 = o.get(s5)), void 0 === t5 && ((this.o = t5 = new CSSStyleSheet()).replaceSync(this.cssText), e5 && o.set(s5, t5));
    }
    return t5;
  }
  toString() {
    return this.cssText;
  }
};
var r = (t5) => new n("string" == typeof t5 ? t5 : t5 + "", void 0, s);
var i = (t5, ...e5) => {
  const o5 = 1 === t5.length ? t5[0] : e5.reduce((e6, s5, o6) => e6 + ((t6) => {
    if (true === t6._$cssResult$) return t6.cssText;
    if ("number" == typeof t6) return t6;
    throw Error("Value passed to 'css' function must be a 'css' function result: " + t6 + ". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.");
  })(s5) + t5[o6 + 1], t5[0]);
  return new n(o5, t5, s);
};
var S = (s5, o5) => {
  if (e) s5.adoptedStyleSheets = o5.map((t5) => t5 instanceof CSSStyleSheet ? t5 : t5.styleSheet);
  else for (const e5 of o5) {
    const o6 = document.createElement("style"), n4 = t.litNonce;
    void 0 !== n4 && o6.setAttribute("nonce", n4), o6.textContent = e5.cssText, s5.appendChild(o6);
  }
};
var c = e ? (t5) => t5 : (t5) => t5 instanceof CSSStyleSheet ? ((t6) => {
  let e5 = "";
  for (const s5 of t6.cssRules) e5 += s5.cssText;
  return r(e5);
})(t5) : t5;

// Tools/NewTab/node_modules/.pnpm/@lit+reactive-element@2.1.2/node_modules/@lit/reactive-element/reactive-element.js
var { is: i2, defineProperty: e2, getOwnPropertyDescriptor: h, getOwnPropertyNames: r2, getOwnPropertySymbols: o2, getPrototypeOf: n2 } = Object;
var a = globalThis;
var c2 = a.trustedTypes;
var l = c2 ? c2.emptyScript : "";
var p = a.reactiveElementPolyfillSupport;
var d = (t5, s5) => t5;
var u = { toAttribute(t5, s5) {
  switch (s5) {
    case Boolean:
      t5 = t5 ? l : null;
      break;
    case Object:
    case Array:
      t5 = null == t5 ? t5 : JSON.stringify(t5);
  }
  return t5;
}, fromAttribute(t5, s5) {
  let i7 = t5;
  switch (s5) {
    case Boolean:
      i7 = null !== t5;
      break;
    case Number:
      i7 = null === t5 ? null : Number(t5);
      break;
    case Object:
    case Array:
      try {
        i7 = JSON.parse(t5);
      } catch (t6) {
        i7 = null;
      }
  }
  return i7;
} };
var f = (t5, s5) => !i2(t5, s5);
var b = { attribute: true, type: String, converter: u, reflect: false, useDefault: false, hasChanged: f };
Symbol.metadata ??= Symbol("metadata"), a.litPropertyMetadata ??= /* @__PURE__ */ new WeakMap();
var y = class extends HTMLElement {
  static addInitializer(t5) {
    this._$Ei(), (this.l ??= []).push(t5);
  }
  static get observedAttributes() {
    return this.finalize(), this._$Eh && [...this._$Eh.keys()];
  }
  static createProperty(t5, s5 = b) {
    if (s5.state && (s5.attribute = false), this._$Ei(), this.prototype.hasOwnProperty(t5) && ((s5 = Object.create(s5)).wrapped = true), this.elementProperties.set(t5, s5), !s5.noAccessor) {
      const i7 = Symbol(), h4 = this.getPropertyDescriptor(t5, i7, s5);
      void 0 !== h4 && e2(this.prototype, t5, h4);
    }
  }
  static getPropertyDescriptor(t5, s5, i7) {
    const { get: e5, set: r5 } = h(this.prototype, t5) ?? { get() {
      return this[s5];
    }, set(t6) {
      this[s5] = t6;
    } };
    return { get: e5, set(s6) {
      const h4 = e5?.call(this);
      r5?.call(this, s6), this.requestUpdate(t5, h4, i7);
    }, configurable: true, enumerable: true };
  }
  static getPropertyOptions(t5) {
    return this.elementProperties.get(t5) ?? b;
  }
  static _$Ei() {
    if (this.hasOwnProperty(d("elementProperties"))) return;
    const t5 = n2(this);
    t5.finalize(), void 0 !== t5.l && (this.l = [...t5.l]), this.elementProperties = new Map(t5.elementProperties);
  }
  static finalize() {
    if (this.hasOwnProperty(d("finalized"))) return;
    if (this.finalized = true, this._$Ei(), this.hasOwnProperty(d("properties"))) {
      const t6 = this.properties, s5 = [...r2(t6), ...o2(t6)];
      for (const i7 of s5) this.createProperty(i7, t6[i7]);
    }
    const t5 = this[Symbol.metadata];
    if (null !== t5) {
      const s5 = litPropertyMetadata.get(t5);
      if (void 0 !== s5) for (const [t6, i7] of s5) this.elementProperties.set(t6, i7);
    }
    this._$Eh = /* @__PURE__ */ new Map();
    for (const [t6, s5] of this.elementProperties) {
      const i7 = this._$Eu(t6, s5);
      void 0 !== i7 && this._$Eh.set(i7, t6);
    }
    this.elementStyles = this.finalizeStyles(this.styles);
  }
  static finalizeStyles(s5) {
    const i7 = [];
    if (Array.isArray(s5)) {
      const e5 = new Set(s5.flat(1 / 0).reverse());
      for (const s6 of e5) i7.unshift(c(s6));
    } else void 0 !== s5 && i7.push(c(s5));
    return i7;
  }
  static _$Eu(t5, s5) {
    const i7 = s5.attribute;
    return false === i7 ? void 0 : "string" == typeof i7 ? i7 : "string" == typeof t5 ? t5.toLowerCase() : void 0;
  }
  constructor() {
    super(), this._$Ep = void 0, this.isUpdatePending = false, this.hasUpdated = false, this._$Em = null, this._$Ev();
  }
  _$Ev() {
    this._$ES = new Promise((t5) => this.enableUpdating = t5), this._$AL = /* @__PURE__ */ new Map(), this._$E_(), this.requestUpdate(), this.constructor.l?.forEach((t5) => t5(this));
  }
  addController(t5) {
    (this._$EO ??= /* @__PURE__ */ new Set()).add(t5), void 0 !== this.renderRoot && this.isConnected && t5.hostConnected?.();
  }
  removeController(t5) {
    this._$EO?.delete(t5);
  }
  _$E_() {
    const t5 = /* @__PURE__ */ new Map(), s5 = this.constructor.elementProperties;
    for (const i7 of s5.keys()) this.hasOwnProperty(i7) && (t5.set(i7, this[i7]), delete this[i7]);
    t5.size > 0 && (this._$Ep = t5);
  }
  createRenderRoot() {
    const t5 = this.shadowRoot ?? this.attachShadow(this.constructor.shadowRootOptions);
    return S(t5, this.constructor.elementStyles), t5;
  }
  connectedCallback() {
    this.renderRoot ??= this.createRenderRoot(), this.enableUpdating(true), this._$EO?.forEach((t5) => t5.hostConnected?.());
  }
  enableUpdating(t5) {
  }
  disconnectedCallback() {
    this._$EO?.forEach((t5) => t5.hostDisconnected?.());
  }
  attributeChangedCallback(t5, s5, i7) {
    this._$AK(t5, i7);
  }
  _$ET(t5, s5) {
    const i7 = this.constructor.elementProperties.get(t5), e5 = this.constructor._$Eu(t5, i7);
    if (void 0 !== e5 && true === i7.reflect) {
      const h4 = (void 0 !== i7.converter?.toAttribute ? i7.converter : u).toAttribute(s5, i7.type);
      this._$Em = t5, null == h4 ? this.removeAttribute(e5) : this.setAttribute(e5, h4), this._$Em = null;
    }
  }
  _$AK(t5, s5) {
    const i7 = this.constructor, e5 = i7._$Eh.get(t5);
    if (void 0 !== e5 && this._$Em !== e5) {
      const t6 = i7.getPropertyOptions(e5), h4 = "function" == typeof t6.converter ? { fromAttribute: t6.converter } : void 0 !== t6.converter?.fromAttribute ? t6.converter : u;
      this._$Em = e5;
      const r5 = h4.fromAttribute(s5, t6.type);
      this[e5] = r5 ?? this._$Ej?.get(e5) ?? r5, this._$Em = null;
    }
  }
  requestUpdate(t5, s5, i7, e5 = false, h4) {
    if (void 0 !== t5) {
      const r5 = this.constructor;
      if (false === e5 && (h4 = this[t5]), i7 ??= r5.getPropertyOptions(t5), !((i7.hasChanged ?? f)(h4, s5) || i7.useDefault && i7.reflect && h4 === this._$Ej?.get(t5) && !this.hasAttribute(r5._$Eu(t5, i7)))) return;
      this.C(t5, s5, i7);
    }
    false === this.isUpdatePending && (this._$ES = this._$EP());
  }
  C(t5, s5, { useDefault: i7, reflect: e5, wrapped: h4 }, r5) {
    i7 && !(this._$Ej ??= /* @__PURE__ */ new Map()).has(t5) && (this._$Ej.set(t5, r5 ?? s5 ?? this[t5]), true !== h4 || void 0 !== r5) || (this._$AL.has(t5) || (this.hasUpdated || i7 || (s5 = void 0), this._$AL.set(t5, s5)), true === e5 && this._$Em !== t5 && (this._$Eq ??= /* @__PURE__ */ new Set()).add(t5));
  }
  async _$EP() {
    this.isUpdatePending = true;
    try {
      await this._$ES;
    } catch (t6) {
      Promise.reject(t6);
    }
    const t5 = this.scheduleUpdate();
    return null != t5 && await t5, !this.isUpdatePending;
  }
  scheduleUpdate() {
    return this.performUpdate();
  }
  performUpdate() {
    if (!this.isUpdatePending) return;
    if (!this.hasUpdated) {
      if (this.renderRoot ??= this.createRenderRoot(), this._$Ep) {
        for (const [t7, s6] of this._$Ep) this[t7] = s6;
        this._$Ep = void 0;
      }
      const t6 = this.constructor.elementProperties;
      if (t6.size > 0) for (const [s6, i7] of t6) {
        const { wrapped: t7 } = i7, e5 = this[s6];
        true !== t7 || this._$AL.has(s6) || void 0 === e5 || this.C(s6, void 0, i7, e5);
      }
    }
    let t5 = false;
    const s5 = this._$AL;
    try {
      t5 = this.shouldUpdate(s5), t5 ? (this.willUpdate(s5), this._$EO?.forEach((t6) => t6.hostUpdate?.()), this.update(s5)) : this._$EM();
    } catch (s6) {
      throw t5 = false, this._$EM(), s6;
    }
    t5 && this._$AE(s5);
  }
  willUpdate(t5) {
  }
  _$AE(t5) {
    this._$EO?.forEach((t6) => t6.hostUpdated?.()), this.hasUpdated || (this.hasUpdated = true, this.firstUpdated(t5)), this.updated(t5);
  }
  _$EM() {
    this._$AL = /* @__PURE__ */ new Map(), this.isUpdatePending = false;
  }
  get updateComplete() {
    return this.getUpdateComplete();
  }
  getUpdateComplete() {
    return this._$ES;
  }
  shouldUpdate(t5) {
    return true;
  }
  update(t5) {
    this._$Eq &&= this._$Eq.forEach((t6) => this._$ET(t6, this[t6])), this._$EM();
  }
  updated(t5) {
  }
  firstUpdated(t5) {
  }
};
y.elementStyles = [], y.shadowRootOptions = { mode: "open" }, y[d("elementProperties")] = /* @__PURE__ */ new Map(), y[d("finalized")] = /* @__PURE__ */ new Map(), p?.({ ReactiveElement: y }), (a.reactiveElementVersions ??= []).push("2.1.2");

// Tools/NewTab/node_modules/.pnpm/lit-html@3.3.3/node_modules/lit-html/lit-html.js
var t2 = globalThis;
var i3 = (t5) => t5;
var s2 = t2.trustedTypes;
var e3 = s2 ? s2.createPolicy("lit-html", { createHTML: (t5) => t5 }) : void 0;
var h2 = "$lit$";
var o3 = `lit$${Math.random().toFixed(9).slice(2)}$`;
var n3 = "?" + o3;
var r3 = `<${n3}>`;
var l2 = document;
var c3 = () => l2.createComment("");
var a2 = (t5) => null === t5 || "object" != typeof t5 && "function" != typeof t5;
var u2 = Array.isArray;
var d2 = (t5) => u2(t5) || "function" == typeof t5?.[Symbol.iterator];
var f2 = "[ 	\n\f\r]";
var v = /<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g;
var _ = /-->/g;
var m = />/g;
var p2 = RegExp(`>|${f2}(?:([^\\s"'>=/]+)(${f2}*=${f2}*(?:[^ 	
\f\r"'\`<>=]|("|')|))|$)`, "g");
var g = /'/g;
var $ = /"/g;
var y2 = /^(?:script|style|textarea|title)$/i;
var x = (t5) => (i7, ...s5) => ({ _$litType$: t5, strings: i7, values: s5 });
var b2 = x(1);
var w = x(2);
var T = x(3);
var E = Symbol.for("lit-noChange");
var A = Symbol.for("lit-nothing");
var C = /* @__PURE__ */ new WeakMap();
var P = l2.createTreeWalker(l2, 129);
function V(t5, i7) {
  if (!u2(t5) || !t5.hasOwnProperty("raw")) throw Error("invalid template strings array");
  return void 0 !== e3 ? e3.createHTML(i7) : i7;
}
var N = (t5, i7) => {
  const s5 = t5.length - 1, e5 = [];
  let n4, l3 = 2 === i7 ? "<svg>" : 3 === i7 ? "<math>" : "", c5 = v;
  for (let i8 = 0; i8 < s5; i8++) {
    const s6 = t5[i8];
    let a3, u5, d3 = -1, f4 = 0;
    for (; f4 < s6.length && (c5.lastIndex = f4, u5 = c5.exec(s6), null !== u5); ) f4 = c5.lastIndex, c5 === v ? "!--" === u5[1] ? c5 = _ : void 0 !== u5[1] ? c5 = m : void 0 !== u5[2] ? (y2.test(u5[2]) && (n4 = RegExp("</" + u5[2], "g")), c5 = p2) : void 0 !== u5[3] && (c5 = p2) : c5 === p2 ? ">" === u5[0] ? (c5 = n4 ?? v, d3 = -1) : void 0 === u5[1] ? d3 = -2 : (d3 = c5.lastIndex - u5[2].length, a3 = u5[1], c5 = void 0 === u5[3] ? p2 : '"' === u5[3] ? $ : g) : c5 === $ || c5 === g ? c5 = p2 : c5 === _ || c5 === m ? c5 = v : (c5 = p2, n4 = void 0);
    const x2 = c5 === p2 && t5[i8 + 1].startsWith("/>") ? " " : "";
    l3 += c5 === v ? s6 + r3 : d3 >= 0 ? (e5.push(a3), s6.slice(0, d3) + h2 + s6.slice(d3) + o3 + x2) : s6 + o3 + (-2 === d3 ? i8 : x2);
  }
  return [V(t5, l3 + (t5[s5] || "<?>") + (2 === i7 ? "</svg>" : 3 === i7 ? "</math>" : "")), e5];
};
var S2 = class _S {
  constructor({ strings: t5, _$litType$: i7 }, e5) {
    let r5;
    this.parts = [];
    let l3 = 0, a3 = 0;
    const u5 = t5.length - 1, d3 = this.parts, [f4, v3] = N(t5, i7);
    if (this.el = _S.createElement(f4, e5), P.currentNode = this.el.content, 2 === i7 || 3 === i7) {
      const t6 = this.el.content.firstChild;
      t6.replaceWith(...t6.childNodes);
    }
    for (; null !== (r5 = P.nextNode()) && d3.length < u5; ) {
      if (1 === r5.nodeType) {
        if (r5.hasAttributes()) for (const t6 of r5.getAttributeNames()) if (t6.endsWith(h2)) {
          const i8 = v3[a3++], s5 = r5.getAttribute(t6).split(o3), e6 = /([.?@])?(.*)/.exec(i8);
          d3.push({ type: 1, index: l3, name: e6[2], strings: s5, ctor: "." === e6[1] ? I : "?" === e6[1] ? L : "@" === e6[1] ? z : H }), r5.removeAttribute(t6);
        } else t6.startsWith(o3) && (d3.push({ type: 6, index: l3 }), r5.removeAttribute(t6));
        if (y2.test(r5.tagName)) {
          const t6 = r5.textContent.split(o3), i8 = t6.length - 1;
          if (i8 > 0) {
            r5.textContent = s2 ? s2.emptyScript : "";
            for (let s5 = 0; s5 < i8; s5++) r5.append(t6[s5], c3()), P.nextNode(), d3.push({ type: 2, index: ++l3 });
            r5.append(t6[i8], c3());
          }
        }
      } else if (8 === r5.nodeType) if (r5.data === n3) d3.push({ type: 2, index: l3 });
      else {
        let t6 = -1;
        for (; -1 !== (t6 = r5.data.indexOf(o3, t6 + 1)); ) d3.push({ type: 7, index: l3 }), t6 += o3.length - 1;
      }
      l3++;
    }
  }
  static createElement(t5, i7) {
    const s5 = l2.createElement("template");
    return s5.innerHTML = t5, s5;
  }
};
function M(t5, i7, s5 = t5, e5) {
  if (i7 === E) return i7;
  let h4 = void 0 !== e5 ? s5._$Co?.[e5] : s5._$Cl;
  const o5 = a2(i7) ? void 0 : i7._$litDirective$;
  return h4?.constructor !== o5 && (h4?._$AO?.(false), void 0 === o5 ? h4 = void 0 : (h4 = new o5(t5), h4._$AT(t5, s5, e5)), void 0 !== e5 ? (s5._$Co ??= [])[e5] = h4 : s5._$Cl = h4), void 0 !== h4 && (i7 = M(t5, h4._$AS(t5, i7.values), h4, e5)), i7;
}
var R = class {
  constructor(t5, i7) {
    this._$AV = [], this._$AN = void 0, this._$AD = t5, this._$AM = i7;
  }
  get parentNode() {
    return this._$AM.parentNode;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  u(t5) {
    const { el: { content: i7 }, parts: s5 } = this._$AD, e5 = (t5?.creationScope ?? l2).importNode(i7, true);
    P.currentNode = e5;
    let h4 = P.nextNode(), o5 = 0, n4 = 0, r5 = s5[0];
    for (; void 0 !== r5; ) {
      if (o5 === r5.index) {
        let i8;
        2 === r5.type ? i8 = new k(h4, h4.nextSibling, this, t5) : 1 === r5.type ? i8 = new r5.ctor(h4, r5.name, r5.strings, this, t5) : 6 === r5.type && (i8 = new Z(h4, this, t5)), this._$AV.push(i8), r5 = s5[++n4];
      }
      o5 !== r5?.index && (h4 = P.nextNode(), o5++);
    }
    return P.currentNode = l2, e5;
  }
  p(t5) {
    let i7 = 0;
    for (const s5 of this._$AV) void 0 !== s5 && (void 0 !== s5.strings ? (s5._$AI(t5, s5, i7), i7 += s5.strings.length - 2) : s5._$AI(t5[i7])), i7++;
  }
};
var k = class _k {
  get _$AU() {
    return this._$AM?._$AU ?? this._$Cv;
  }
  constructor(t5, i7, s5, e5) {
    this.type = 2, this._$AH = A, this._$AN = void 0, this._$AA = t5, this._$AB = i7, this._$AM = s5, this.options = e5, this._$Cv = e5?.isConnected ?? true;
  }
  get parentNode() {
    let t5 = this._$AA.parentNode;
    const i7 = this._$AM;
    return void 0 !== i7 && 11 === t5?.nodeType && (t5 = i7.parentNode), t5;
  }
  get startNode() {
    return this._$AA;
  }
  get endNode() {
    return this._$AB;
  }
  _$AI(t5, i7 = this) {
    t5 = M(this, t5, i7), a2(t5) ? t5 === A || null == t5 || "" === t5 ? (this._$AH !== A && this._$AR(), this._$AH = A) : t5 !== this._$AH && t5 !== E && this._(t5) : void 0 !== t5._$litType$ ? this.$(t5) : void 0 !== t5.nodeType ? this.T(t5) : d2(t5) ? this.k(t5) : this._(t5);
  }
  O(t5) {
    return this._$AA.parentNode.insertBefore(t5, this._$AB);
  }
  T(t5) {
    this._$AH !== t5 && (this._$AR(), this._$AH = this.O(t5));
  }
  _(t5) {
    this._$AH !== A && a2(this._$AH) ? this._$AA.nextSibling.data = t5 : this.T(l2.createTextNode(t5)), this._$AH = t5;
  }
  $(t5) {
    const { values: i7, _$litType$: s5 } = t5, e5 = "number" == typeof s5 ? this._$AC(t5) : (void 0 === s5.el && (s5.el = S2.createElement(V(s5.h, s5.h[0]), this.options)), s5);
    if (this._$AH?._$AD === e5) this._$AH.p(i7);
    else {
      const t6 = new R(e5, this), s6 = t6.u(this.options);
      t6.p(i7), this.T(s6), this._$AH = t6;
    }
  }
  _$AC(t5) {
    let i7 = C.get(t5.strings);
    return void 0 === i7 && C.set(t5.strings, i7 = new S2(t5)), i7;
  }
  k(t5) {
    u2(this._$AH) || (this._$AH = [], this._$AR());
    const i7 = this._$AH;
    let s5, e5 = 0;
    for (const h4 of t5) e5 === i7.length ? i7.push(s5 = new _k(this.O(c3()), this.O(c3()), this, this.options)) : s5 = i7[e5], s5._$AI(h4), e5++;
    e5 < i7.length && (this._$AR(s5 && s5._$AB.nextSibling, e5), i7.length = e5);
  }
  _$AR(t5 = this._$AA.nextSibling, s5) {
    for (this._$AP?.(false, true, s5); t5 !== this._$AB; ) {
      const s6 = i3(t5).nextSibling;
      i3(t5).remove(), t5 = s6;
    }
  }
  setConnected(t5) {
    void 0 === this._$AM && (this._$Cv = t5, this._$AP?.(t5));
  }
};
var H = class {
  get tagName() {
    return this.element.tagName;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  constructor(t5, i7, s5, e5, h4) {
    this.type = 1, this._$AH = A, this._$AN = void 0, this.element = t5, this.name = i7, this._$AM = e5, this.options = h4, s5.length > 2 || "" !== s5[0] || "" !== s5[1] ? (this._$AH = Array(s5.length - 1).fill(new String()), this.strings = s5) : this._$AH = A;
  }
  _$AI(t5, i7 = this, s5, e5) {
    const h4 = this.strings;
    let o5 = false;
    if (void 0 === h4) t5 = M(this, t5, i7, 0), o5 = !a2(t5) || t5 !== this._$AH && t5 !== E, o5 && (this._$AH = t5);
    else {
      const e6 = t5;
      let n4, r5;
      for (t5 = h4[0], n4 = 0; n4 < h4.length - 1; n4++) r5 = M(this, e6[s5 + n4], i7, n4), r5 === E && (r5 = this._$AH[n4]), o5 ||= !a2(r5) || r5 !== this._$AH[n4], r5 === A ? t5 = A : t5 !== A && (t5 += (r5 ?? "") + h4[n4 + 1]), this._$AH[n4] = r5;
    }
    o5 && !e5 && this.j(t5);
  }
  j(t5) {
    t5 === A ? this.element.removeAttribute(this.name) : this.element.setAttribute(this.name, t5 ?? "");
  }
};
var I = class extends H {
  constructor() {
    super(...arguments), this.type = 3;
  }
  j(t5) {
    this.element[this.name] = t5 === A ? void 0 : t5;
  }
};
var L = class extends H {
  constructor() {
    super(...arguments), this.type = 4;
  }
  j(t5) {
    this.element.toggleAttribute(this.name, !!t5 && t5 !== A);
  }
};
var z = class extends H {
  constructor(t5, i7, s5, e5, h4) {
    super(t5, i7, s5, e5, h4), this.type = 5;
  }
  _$AI(t5, i7 = this) {
    if ((t5 = M(this, t5, i7, 0) ?? A) === E) return;
    const s5 = this._$AH, e5 = t5 === A && s5 !== A || t5.capture !== s5.capture || t5.once !== s5.once || t5.passive !== s5.passive, h4 = t5 !== A && (s5 === A || e5);
    e5 && this.element.removeEventListener(this.name, this, s5), h4 && this.element.addEventListener(this.name, this, t5), this._$AH = t5;
  }
  handleEvent(t5) {
    "function" == typeof this._$AH ? this._$AH.call(this.options?.host ?? this.element, t5) : this._$AH.handleEvent(t5);
  }
};
var Z = class {
  constructor(t5, i7, s5) {
    this.element = t5, this.type = 6, this._$AN = void 0, this._$AM = i7, this.options = s5;
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  _$AI(t5) {
    M(this, t5);
  }
};
var j = { M: h2, P: o3, A: n3, C: 1, L: N, R, D: d2, V: M, I: k, H, N: L, U: z, B: I, F: Z };
var B = t2.litHtmlPolyfillSupport;
B?.(S2, k), (t2.litHtmlVersions ??= []).push("3.3.3");
var D = (t5, i7, s5) => {
  const e5 = s5?.renderBefore ?? i7;
  let h4 = e5._$litPart$;
  if (void 0 === h4) {
    const t6 = s5?.renderBefore ?? null;
    e5._$litPart$ = h4 = new k(i7.insertBefore(c3(), t6), t6, void 0, s5 ?? {});
  }
  return h4._$AI(t5), h4;
};

// Tools/NewTab/node_modules/.pnpm/lit-element@4.2.2/node_modules/lit-element/lit-element.js
var s3 = globalThis;
var i4 = class extends y {
  constructor() {
    super(...arguments), this.renderOptions = { host: this }, this._$Do = void 0;
  }
  createRenderRoot() {
    const t5 = super.createRenderRoot();
    return this.renderOptions.renderBefore ??= t5.firstChild, t5;
  }
  update(t5) {
    const r5 = this.render();
    this.hasUpdated || (this.renderOptions.isConnected = this.isConnected), super.update(t5), this._$Do = D(r5, this.renderRoot, this.renderOptions);
  }
  connectedCallback() {
    super.connectedCallback(), this._$Do?.setConnected(true);
  }
  disconnectedCallback() {
    super.disconnectedCallback(), this._$Do?.setConnected(false);
  }
  render() {
    return E;
  }
};
i4._$litElement$ = true, i4["finalized"] = true, s3.litElementHydrateSupport?.({ LitElement: i4 });
var o4 = s3.litElementPolyfillSupport;
o4?.({ LitElement: i4 });
(s3.litElementVersions ??= []).push("4.2.2");

// Tools/NewTab/node_modules/.pnpm/lit-html@3.3.3/node_modules/lit-html/directive.js
var t3 = { ATTRIBUTE: 1, CHILD: 2, PROPERTY: 3, BOOLEAN_ATTRIBUTE: 4, EVENT: 5, ELEMENT: 6 };
var e4 = (t5) => (...e5) => ({ _$litDirective$: t5, values: e5 });
var i5 = class {
  constructor(t5) {
  }
  get _$AU() {
    return this._$AM._$AU;
  }
  _$AT(t5, e5, i7) {
    this._$Ct = t5, this._$AM = e5, this._$Ci = i7;
  }
  _$AS(t5, e5) {
    return this.update(t5, e5);
  }
  update(t5, e5) {
    return this.render(...e5);
  }
};

// Tools/NewTab/node_modules/.pnpm/lit-html@3.3.3/node_modules/lit-html/directive-helpers.js
var { I: t4 } = j;
var i6 = (o5) => o5;
var s4 = () => document.createComment("");
var v2 = (o5, n4, e5) => {
  const l3 = o5._$AA.parentNode, d3 = void 0 === n4 ? o5._$AB : n4._$AA;
  if (void 0 === e5) {
    const i7 = l3.insertBefore(s4(), d3), n5 = l3.insertBefore(s4(), d3);
    e5 = new t4(i7, n5, o5, o5.options);
  } else {
    const t5 = e5._$AB.nextSibling, n5 = e5._$AM, c5 = n5 !== o5;
    if (c5) {
      let t6;
      e5._$AQ?.(o5), e5._$AM = o5, void 0 !== e5._$AP && (t6 = o5._$AU) !== n5._$AU && e5._$AP(t6);
    }
    if (t5 !== d3 || c5) {
      let o6 = e5._$AA;
      for (; o6 !== t5; ) {
        const t6 = i6(o6).nextSibling;
        i6(l3).insertBefore(o6, d3), o6 = t6;
      }
    }
  }
  return e5;
};
var u3 = (o5, t5, i7 = o5) => (o5._$AI(t5, i7), o5);
var m2 = {};
var p3 = (o5, t5 = m2) => o5._$AH = t5;
var M2 = (o5) => o5._$AH;
var h3 = (o5) => {
  o5._$AR(), o5._$AA.remove();
};

// Tools/NewTab/node_modules/.pnpm/lit-html@3.3.3/node_modules/lit-html/directives/repeat.js
var u4 = (e5, s5, t5) => {
  const r5 = /* @__PURE__ */ new Map();
  for (let l3 = s5; l3 <= t5; l3++) r5.set(e5[l3], l3);
  return r5;
};
var c4 = e4(class extends i5 {
  constructor(e5) {
    if (super(e5), e5.type !== t3.CHILD) throw Error("repeat() can only be used in text expressions");
  }
  dt(e5, s5, t5) {
    let r5;
    void 0 === t5 ? t5 = s5 : void 0 !== s5 && (r5 = s5);
    const l3 = [], o5 = [];
    let i7 = 0;
    for (const s6 of e5) l3[i7] = r5 ? r5(s6, i7) : i7, o5[i7] = t5(s6, i7), i7++;
    return { values: o5, keys: l3 };
  }
  render(e5, s5, t5) {
    return this.dt(e5, s5, t5).values;
  }
  update(s5, [t5, r5, c5]) {
    const d3 = M2(s5), { values: p4, keys: a3 } = this.dt(t5, r5, c5);
    if (!Array.isArray(d3)) return this.ut = a3, p4;
    const h4 = this.ut ??= [], v3 = [];
    let m3, y3, x2 = 0, j2 = d3.length - 1, k2 = 0, w2 = p4.length - 1;
    for (; x2 <= j2 && k2 <= w2; ) if (null === d3[x2]) x2++;
    else if (null === d3[j2]) j2--;
    else if (h4[x2] === a3[k2]) v3[k2] = u3(d3[x2], p4[k2]), x2++, k2++;
    else if (h4[j2] === a3[w2]) v3[w2] = u3(d3[j2], p4[w2]), j2--, w2--;
    else if (h4[x2] === a3[w2]) v3[w2] = u3(d3[x2], p4[w2]), v2(s5, v3[w2 + 1], d3[x2]), x2++, w2--;
    else if (h4[j2] === a3[k2]) v3[k2] = u3(d3[j2], p4[k2]), v2(s5, d3[x2], d3[j2]), j2--, k2++;
    else if (void 0 === m3 && (m3 = u4(a3, k2, w2), y3 = u4(h4, x2, j2)), m3.has(h4[x2])) if (m3.has(h4[j2])) {
      const e5 = y3.get(a3[k2]), t6 = void 0 !== e5 ? d3[e5] : null;
      if (null === t6) {
        const e6 = v2(s5, d3[x2]);
        u3(e6, p4[k2]), v3[k2] = e6;
      } else v3[k2] = u3(t6, p4[k2]), v2(s5, d3[x2], t6), d3[e5] = null;
      k2++;
    } else h3(d3[j2]), j2--;
    else h3(d3[x2]), x2++;
    for (; k2 <= w2; ) {
      const e5 = v2(s5, v3[w2 + 1]);
      u3(e5, p4[k2]), v3[k2++] = e5;
    }
    for (; x2 <= j2; ) {
      const e5 = d3[x2++];
      null !== e5 && h3(e5);
    }
    return this.ut = a3, p3(s5, v3), E;
  }
});

// Tools/NewTab/chromium-components.js
function drawMonogram(data) {
  const size = Math.ceil(24 * (window.devicePixelRatio || 1));
  const canvas = document.createElement("canvas");
  canvas.width = size;
  canvas.height = size;
  const ctx = canvas.getContext("2d");
  ctx.fillStyle = data.color;
  ctx.beginPath();
  ctx.roundRect(0, 0, size, size, Math.floor(size / 2));
  ctx.fill();
  ctx.fillStyle = "#fff";
  ctx.font = `600 ${Math.trunc(size * 0.5)}px "Segoe UI"`;
  ctx.textAlign = "center";
  ctx.textBaseline = "alphabetic";
  const metrics = ctx.measureText(data.text);
  const ascent = metrics.fontBoundingBoxAscent, descent = metrics.fontBoundingBoxDescent;
  ctx.fillText(data.text, size / 2, (size - ascent - descent) / 2 + ascent);
  return canvas.toDataURL("image/png");
}
var loadTimeData = {
  data: {
    addLinkTitle: "Add shortcut",
    editLinkTitle: "Edit shortcut",
    nameField: "Name",
    urlField: "URL",
    linkCancel: "Cancel",
    linkDone: "Done",
    linkRemove: "Remove",
    linkRemoveA11y: "Remove $1",
    shortcutMoreActions: "More actions for $1",
    shortcutAlreadyExists: "Shortcut already exists",
    invalidUrl: "Enter a valid URL",
    linkAddedMsg: "Shortcut added",
    linkEditedMsg: "Shortcut edited",
    linkRemovedMsg: "Shortcut removed",
    linkCantCreate: "Can\u2019t create shortcut",
    linkCantEdit: "Can\u2019t edit shortcut",
    undo: "Undo",
    undoDescription: "Undo last action",
    restoreDefaultLinks: "Restore default shortcuts",
    restoreThumbnailsShort: "Restore all",
    showMore: "Show more",
    showLess: "Show less",
    searchBoxHint: "Search Google or type a URL",
    searchboxSeparator: " \u2013 ",
    removeSuggestion: "Remove suggestion",
    close: "Close",
    invalid: "Invalid",
    isWindows: true,
    realboxVirtualFocusNavigation: false,
    mostVisitedHighDpiFaviconsEnabled: true,
    reportMetrics: false
  },
  isInitialized() {
    return true;
  },
  getBoolean(name) {
    return !!this.data[name];
  },
  getInteger(name) {
    return this.data[name] ?? 0;
  },
  getString(name) {
    return this.data[name] ?? name;
  },
  getStringF(name, ...args) {
    return this.getString(name).replace(/\$(\d)/g, (_2, n4) => args[+n4 - 1] ?? "");
  },
  valueExists(name) {
    return name in this.data;
  },
  getValue(name) {
    return this.data[name];
  },
  overrideValues(data) {
    Object.assign(this.data, data);
  }
};
var TileSource = { TOP_SITES: 0, POPULAR: 1, POPULAR_BAKED_IN: 2, CUSTOM_LINKS: 3, ALLOWLIST: 4, HOMEPAGE: 5, ENTERPRISE_SHORTCUTS: 6 };
var TextDirection = { UNKNOWN_DIRECTION: 0, RIGHT_TO_LEFT: 1, LEFT_TO_RIGHT: 2 };
var KeywordType = { kChip: 0, kInKeyword: 1, kInstant: 2 };
var SideType = { kDefaultPrimary: 0, kSecondary: 1 };
var RenderType = { kDefaultVertical: 0, kHorizontal: 1, kGrid: 2 };
var SelectionLineState = { kNormal: 1, kKeywordMode: 2, kFocusedButtonAction: 3, kFocusedButtonRemoveSuggestion: 4, kFocusedButtonAim: 5, kFocusedButtonContextEntrypoint: 6, kCtrlEnter: 7 };
var SelectionDirection = { kForward: 1, kBackward: 2 };
var SelectionStep = { kWholeLine: 1, kStateOrLine: 2, kAllLines: 3 };
var NavigationPredictor = { kMouseDown: 0, kMouseOver: 1, kTouchDown: 2, kUpOrDownArrowButton: 3 };
var SuggestInventory = { kDefault: 0 };
var InputMethod = { kKeyboard: 0 };
var MetricsReporterImpl = { getInstance: () => ({ hasLocalMark: () => false, mark() {
}, clearMark() {
}, measure: async () => 0, reportTime() {
} }) };
var listenerId = 0;
var Signal = class {
  listeners = /* @__PURE__ */ new Map();
  addListener(fn) {
    const id = ++listenerId;
    this.listeners.set(id, fn);
    return id;
  }
  emit(...args) {
    for (const fn of this.listeners.values()) fn(...args);
  }
};
var callbackRouter = {
  setMostVisitedInfo: new Signal(),
  onMostVisitedTilesAutoRemoval: new Signal(),
  setInputText: new Signal(),
  autocompleteResultChanged: new Signal(),
  setKeywordSpaceTriggeringEnabled: new Signal(),
  setAvailableKeywordModels: new Signal(),
  removeListener(id) {
    for (const value of Object.values(this)) if (value instanceof Signal) value.listeners.delete(id);
  }
};
var hostEvents = new EventTarget();
var messageId = 0;
function send(type, data = {}) {
  const id = ++messageId;
  window.chrome?.webview?.postMessage({ channel: "quartz-newtab", id, type, ...data });
  return id;
}
window.chrome?.webview?.addEventListener("message", ({ data }) => hostEvents.dispatchEvent(new CustomEvent(data.type, { detail: data })));
var storageKey = "quartz.newtab.shortcuts.v1";
var undo = null;
function normalizeUrl(value) {
  try {
    const url = new URL(value.includes("://") ? value : `https://${value}/`);
    return ["http:", "https:"].includes(url.protocol) && !url.username && !url.password ? url.href : null;
  } catch {
    return null;
  }
}
function saved() {
  const data = JSON.parse(localStorage.getItem(storageKey) || "[]");
  if (!Array.isArray(data) || data.length > 10 || data.some((s5) => !s5 || typeof s5.id !== "string" || typeof s5.name !== "string" || typeof s5.url !== "string" || !normalizeUrl(s5.url))) throw Error("Invalid shortcuts");
  return data;
}
function publish() {
  let items;
  try {
    items = saved();
  } catch {
    items = [];
    hostEvents.dispatchEvent(new CustomEvent("storage-error"));
  }
  callbackRouter.setMostVisitedInfo.emit({
    visible: true,
    customLinksEnabled: true,
    enterpriseShortcutsEnabled: false,
    tiles: items.map((s5) => ({
      id: s5.id,
      url: s5.url,
      title: s5.name,
      titleDirection: TextDirection.LEFT_TO_RIGHT,
      isQueryTile: false,
      allowUserEdit: true,
      allowUserDelete: true,
      source: TileSource.CUSTOM_LINKS,
      titleSource: 0
    }))
  });
  if (items.length) send("icons", { urls: items.map((s5) => s5.url) });
}
function change(edit) {
  try {
    const before = saved(), items = before.map((s5) => ({ ...s5 }));
    if (edit(items) === false) return { success: false };
    const after = JSON.stringify(items);
    localStorage.setItem(storageKey, after);
    undo = { before: JSON.stringify(before), after };
    publish();
    return { success: true };
  } catch {
    hostEvents.dispatchEvent(new CustomEvent("storage-error"));
    return { success: false };
  }
}
function navigate(url, button = 0, ctrl = false, shift = false) {
  if (button === 1 || ctrl || shift) window.open(url, "_blank");
  else send("navigate", { text: url });
}
var handler = {
  getMostVisitedExpandedState: async () => ({ isExpanded: false }),
  setMostVisitedExpandedState() {
  },
  updateMostVisitedInfo: publish,
  addMostVisitedTile: async (url, title) => change((items) => {
    url = normalizeUrl(url);
    if (!url || items.length >= 10 || items.some((s5) => normalizeUrl(s5.url) === url)) return false;
    items.push({ id: crypto.randomUUID(), name: title, url });
  }),
  updateMostVisitedTile: async (tile, url, title) => change((items) => {
    const item = items.find((s5) => s5.id === tile.id);
    url = normalizeUrl(url);
    if (!item || !url || items.some((s5) => s5.id !== item.id && normalizeUrl(s5.url) === url)) return false;
    item.name = title;
    item.url = url;
  }),
  deleteMostVisitedTile: (tile) => change((items) => {
    const i7 = items.findIndex((s5) => s5.id === tile.id);
    if (i7 < 0) return false;
    items.splice(i7, 1);
  }),
  reorderMostVisitedTile: (tile, index) => change((items) => {
    const i7 = items.findIndex((s5) => s5.id === tile.id);
    if (i7 < 0) return false;
    items.splice(index, 0, items.splice(i7, 1)[0]);
  }),
  undoMostVisitedTileAction() {
    try {
      if (undo && localStorage.getItem(storageKey) === undo.after) {
        localStorage.setItem(storageKey, undo.before);
        undo = null;
        publish();
      }
    } catch {
      hostEvents.dispatchEvent(new CustomEvent("storage-error"));
    }
  },
  restoreMostVisitedDefaults: () => change((items) => {
    items.length = 0;
  }),
  onMostVisitedTileNavigation: (tile, index, button, alt, ctrl, meta, shift) => navigate(tile.url, button, ctrl || meta, shift),
  onMostVisitedTilesRendered() {
  },
  preconnectMostVisitedTile() {
  },
  prefetchMostVisitedTile() {
  },
  prerenderMostVisitedTile() {
  },
  cancelPrerender() {
  },
  openAutocompleteMatch(index, url, showing, button, modifiers) {
    hostEvents.dispatchEvent(new CustomEvent("open-match", { detail: { index, button, modifiers } }));
  },
  onNavigationLikely() {
  },
  deleteAutocompleteMatch(index) {
    hostEvents.dispatchEvent(new CustomEvent("remove-match", { detail: { index } }));
  }
};
var browserProxyFactory = { getInstance: () => ({ handler, callbackRouter }) };
var SearchboxBrowserProxy = browserProxyFactory;
window.addEventListener("storage", (e5) => {
  if (e5.key === storageKey || e5.key === null) publish();
});
var favicons = /* @__PURE__ */ new Map();
var monograms = /* @__PURE__ */ new Map();
var emptyIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";
function faviconUrl(url) {
  return favicons.get(url) || monograms.get(url) || emptyIcon;
}
function getFaviconUrl(url) {
  return favicons.get(url) || "";
}
hostEvents.addEventListener("icons", ({ detail }) => {
  for (const [url, image] of Object.entries(detail.icons || {}))
    if (typeof image === "string" && image.startsWith("data:image/x-icon;base64,")) favicons.set(url, image);
  for (const [url, data] of Object.entries(detail.fallbacks || {})) monograms.set(url, drawMonogram(data));
  hostEvents.dispatchEvent(new CustomEvent("icons-updated"));
});
function createAutocompleteMatch(modifiers = {}) {
  return Object.assign({
    isHidden: false,
    a11yLabel: "",
    actions: [],
    allowedToBeDefaultMatch: false,
    isSearchType: false,
    isEnterpriseSearchAggregatorPeopleType: false,
    swapContentsAndDescription: false,
    showContextualDescription: false,
    supportsDeletion: false,
    suggestionGroupId: -1,
    contents: "",
    contentsClass: [{ offset: 0, style: 0 }],
    description: "",
    descriptionClass: [{ offset: 0, style: 0 }],
    destinationUrl: "",
    inlineAutocompletion: "",
    fillIntoEdit: "",
    iconPath: "",
    iconUrl: "",
    imageDominantColor: "",
    imageUrl: "",
    isContextualSuggestion: false,
    isNoncannedAimSuggestion: false,
    removeButtonA11yLabel: "",
    type: "",
    isTwoRowSuggestion: false,
    tailSuggestCommonPrefix: null,
    keywordModel: null,
    fuseboxAction: null,
    suggestStyle: 0
  }, modifiers);
}
var style = document.createElement("link");
style.rel = "stylesheet";
style.href = "/newtab/chromium/cr_elements/cr_shared_vars.css";
document.head.append(style);
function assert(value, message) {
  if (value) {
    return;
  }
  throw new Error("Assertion failed" + (message ? `: ${message}` : ""));
}
function assertInstanceof(value, type, message) {
  if (value instanceof type) {
    return;
  }
  throw new Error(
    message || `Value ${value} is not of type ${type.name || typeof type}`
  );
}
function assertNotReached(message = "Unreachable code hit") {
  assert(false, message);
}
function assertNotReachedCase(_param, message) {
  assertNotReached(message);
}
var CLASS_NAME = "focus-outline-visible";
var docsToManager = /* @__PURE__ */ new Map();
var FocusOutlineManager = class _FocusOutlineManager {
  // Whether focus change is triggered by a keyboard event.
  focusByKeyboard_ = true;
  classList_;
  /**
   * @param doc The document to attach the focus outline manager to.
   */
  constructor(doc) {
    this.classList_ = doc.documentElement.classList;
    doc.addEventListener("keydown", (e5) => this.onEvent_(true, e5), true);
    doc.addEventListener("mousedown", (e5) => this.onEvent_(false, e5), true);
    this.updateVisibility();
  }
  onEvent_(focusByKeyboard, e5) {
    if (this.focusByKeyboard_ === focusByKeyboard) {
      return;
    }
    if (e5 instanceof KeyboardEvent && e5.repeat) {
      return;
    }
    this.focusByKeyboard_ = focusByKeyboard;
    this.updateVisibility();
  }
  updateVisibility() {
    this.visible = this.focusByKeyboard_;
  }
  /**
   * Whether the focus outline should be visible.
   */
  set visible(visible) {
    this.classList_.toggle(CLASS_NAME, visible);
  }
  get visible() {
    return this.classList_.contains(CLASS_NAME);
  }
  /**
   * Gets a per document singleton focus outline manager.
   * @param doc The document to get the |FocusOutlineManager| for.
   * @return The per document singleton focus outline manager.
   */
  static forDocument(doc) {
    let manager = docsToManager.get(doc);
    if (!manager) {
      manager = new _FocusOutlineManager(doc);
      docsToManager.set(doc, manager);
    }
    return manager;
  }
};
var EventTracker = class _EventTracker {
  listeners_ = [];
  /**
   * Add an event listener - replacement for EventTarget.addEventListener.
   * @param target The DOM target to add a listener to.
   * @param eventType The type of event to subscribe to.
   * @param listener The listener to add.
   * @param capture Whether to invoke during the capture phase. Defaults to
   *     false.
   */
  add(target, eventType, listener, capture = false) {
    const h4 = {
      target,
      eventType,
      listener,
      capture
    };
    this.listeners_.push(h4);
    target.addEventListener(eventType, listener, capture);
  }
  /**
   * Remove any specified event listeners added with this EventTracker.
   * @param target The DOM target to remove a listener from.
   * @param eventType The type of event to remove.
   */
  remove(target, eventType) {
    this.listeners_ = this.listeners_.filter((listener) => {
      if (listener.target === target && (!eventType || listener.eventType === eventType)) {
        _EventTracker.removeEventListener(listener);
        return false;
      }
      return true;
    });
  }
  /** Remove all event listeners added with this EventTracker. */
  removeAll() {
    this.listeners_.forEach(
      (listener) => _EventTracker.removeEventListener(listener)
    );
    this.listeners_ = [];
  }
  /**
   * Remove a single event listener given it's tracking entry. It's up to the
   * caller to ensure the entry is removed from listeners_.
   * @param entry The entry describing the listener to
   * remove.
   */
  static removeEventListener(entry) {
    entry.target.removeEventListener(
      entry.eventType,
      entry.listener,
      entry.capture
    );
  }
};
function getDeepActiveElement() {
  let a3 = document.activeElement;
  while (a3 && a3.shadowRoot && a3.shadowRoot.activeElement) {
    a3 = a3.shadowRoot.activeElement;
  }
  return a3;
}
function isRTL() {
  return document.documentElement.dir === "rtl";
}
function htmlEscape(original) {
  return original.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/"/g, "&quot;").replace(/'/g, "&#39;");
}
function hasKeyModifiers(e5) {
  return e5.altKey || e5.ctrlKey || e5.metaKey || e5.shiftKey;
}
var ACTIVE_CLASS = "focus-row-active";
var FocusRow = class _FocusRow {
  root;
  delegate;
  eventTracker = new EventTracker();
  boundary_;
  /**
   * @param root The root of this focus row. Focus classes are
   *     applied to |root| and all added elements must live within |root|.
   * @param boundary Focus events are ignored outside of this element.
   * @param delegate An optional event delegate.
   */
  constructor(root, boundary, delegate) {
    this.root = root;
    this.boundary_ = boundary || document.documentElement;
    this.delegate = delegate;
  }
  /**
   * Whether it's possible that |element| can be focused.
   */
  static isFocusable(element) {
    if (!element || element.disabled) {
      return false;
    }
    let current2 = element;
    while (true) {
      assertInstanceof(current2, Element);
      const style2 = window.getComputedStyle(current2);
      if (style2.visibility === "hidden" || style2.display === "none") {
        return false;
      }
      const parent = current2.parentNode;
      if (!parent) {
        return false;
      }
      if (parent === current2.ownerDocument || parent instanceof DocumentFragment) {
        return true;
      }
      current2 = parent;
    }
  }
  /**
   * A focus override is a function that returns an element that should gain
   * focus. The element may not be directly selectable for example the element
   * that can gain focus is in a shadow DOM. Allowing an override via a
   * function leaves the details of how the element is retrieved to the
   * component.
   */
  static getFocusableElement(element) {
    const withFocusable = element;
    if (withFocusable.getFocusableElement) {
      return withFocusable.getFocusableElement();
    }
    return element;
  }
  /**
   * Register a new type of focusable element (or add to an existing one).
   *
   * Example: an (X) button might be 'delete' or 'close'.
   *
   * When FocusRow is used within a FocusGrid, these types are used to
   * determine equivalent controls when Up/Down are pressed to change rows.
   *
   * Another example: mutually exclusive controls that hide each other on
   * activation (i.e. Play/Pause) could use the same type (i.e. 'play-pause')
   * to indicate they're equivalent.
   *
   * @param type The type of element to track focus of.
   * @param selectorOrElement The selector of the element
   *    from this row's root, or the element itself.
   * @return Whether a new item was added.
   */
  addItem(type, selectorOrElement) {
    assert(type);
    let element;
    if (typeof selectorOrElement === "string") {
      element = this.root.querySelector(selectorOrElement);
    } else {
      element = selectorOrElement;
    }
    if (!element) {
      return false;
    }
    element.setAttribute("focus-type", type);
    element.tabIndex = this.isActive() ? 0 : -1;
    this.eventTracker.add(element, "blur", this.onBlur_.bind(this));
    this.eventTracker.add(element, "focus", this.onFocus_.bind(this));
    this.eventTracker.add(element, "keydown", this.onKeydown_.bind(this));
    this.eventTracker.add(element, "mousedown", this.onMousedown_.bind(this));
    return true;
  }
  /** Dereferences nodes and removes event handlers. */
  destroy() {
    this.eventTracker.removeAll();
  }
  /**
   * @param sampleElement An element for to find an equivalent
   *     for.
   * @return An equivalent element to focus for
   *     |sampleElement|.
   */
  getCustomEquivalent(_sampleElement) {
    const focusable = this.getFirstFocusable();
    assert(focusable);
    return focusable;
  }
  /**
   * @return All registered elements (regardless of focusability).
   */
  getElements() {
    return Array.from(this.root.querySelectorAll("[focus-type]")).map(_FocusRow.getFocusableElement);
  }
  /**
   * Find the element that best matches |sampleElement|.
   * @param sampleElement An element from a row of the same
   *     type which previously held focus.
   * @return The element that best matches sampleElement.
   */
  getEquivalentElement(sampleElement) {
    if (this.getFocusableElements().indexOf(sampleElement) >= 0) {
      return sampleElement;
    }
    const sampleFocusType = this.getTypeForElement(sampleElement);
    if (sampleFocusType) {
      const sameType = this.getFirstFocusable(sampleFocusType);
      if (sameType) {
        return sameType;
      }
    }
    return this.getCustomEquivalent(sampleElement);
  }
  /**
   * @param type An optional type to search for.
   * @return The first focusable element with |type|.
   */
  getFirstFocusable(type) {
    const element = this.getFocusableElements().find(
      (el) => !type || el.getAttribute("focus-type") === type
    );
    return element || null;
  }
  /** @return Registered, focusable elements. */
  getFocusableElements() {
    return this.getElements().filter(_FocusRow.isFocusable);
  }
  /**
   * @param element An element to determine a focus type for.
   * @return The focus type for |element| or '' if none.
   */
  getTypeForElement(element) {
    return element.getAttribute("focus-type") || "";
  }
  /** @return Whether this row is currently active. */
  isActive() {
    return this.root.classList.contains(ACTIVE_CLASS);
  }
  /**
   * Enables/disables the tabIndex of the focusable elements in the FocusRow.
   * tabIndex can be set properly.
   * @param active True if tab is allowed for this row.
   */
  makeActive(active) {
    if (active === this.isActive()) {
      return;
    }
    this.getElements().forEach(function(element) {
      element.tabIndex = active ? 0 : -1;
    });
    this.root.classList.toggle(ACTIVE_CLASS, active);
  }
  onBlur_(e5) {
    if (!this.boundary_.contains(e5.relatedTarget)) {
      return;
    }
    const currentTarget = e5.currentTarget;
    if (this.getFocusableElements().indexOf(currentTarget) >= 0) {
      this.makeActive(false);
    }
  }
  onFocus_(e5) {
    if (this.delegate) {
      this.delegate.onFocus(this, e5);
    }
  }
  onMousedown_(e5) {
    if (e5.button) {
      return;
    }
    const target = e5.currentTarget;
    if (!target.disabled) {
      target.tabIndex = 0;
    }
  }
  onKeydown_(e5) {
    const elements = this.getFocusableElements();
    const currentElement = _FocusRow.getFocusableElement(
      e5.currentTarget
    );
    const elementIndex = elements.indexOf(currentElement);
    assert(elementIndex >= 0);
    if (this.delegate && this.delegate.onKeydown(this, e5)) {
      return;
    }
    const isShiftTab = !e5.altKey && !e5.ctrlKey && !e5.metaKey && e5.shiftKey && e5.key === "Tab";
    if (hasKeyModifiers(e5) && !isShiftTab) {
      return;
    }
    let index = -1;
    let shouldStopPropagation = true;
    if (isShiftTab) {
      index = elementIndex - 1;
      if (index < 0) {
        return;
      }
    } else if (e5.key === "ArrowLeft") {
      index = elementIndex + (isRTL() ? 1 : -1);
    } else if (e5.key === "ArrowRight") {
      index = elementIndex + (isRTL() ? -1 : 1);
    } else if (e5.key === "Home") {
      index = 0;
    } else if (e5.key === "End") {
      index = elements.length - 1;
    } else {
      shouldStopPropagation = false;
    }
    const elementToFocus = elements[index];
    if (elementToFocus) {
      this.getEquivalentElement(elementToFocus).focus();
      e5.preventDefault();
    }
    if (shouldStopPropagation) {
      e5.stopPropagation();
    }
  }
};
var hideInk = false;
document.addEventListener("pointerdown", function() {
  hideInk = true;
}, true);
document.addEventListener("keydown", function() {
  hideInk = false;
}, true);
function focusWithoutInk(toFocus) {
  if (!("noink" in toFocus) || !hideInk) {
    toFocus.focus();
    return;
  }
  const toFocusWithNoInk = toFocus;
  assert(document === toFocusWithNoInk.ownerDocument);
  const { noink } = toFocusWithNoInk;
  toFocusWithNoInk.noink = true;
  toFocusWithNoInk.focus();
  toFocusWithNoInk.noink = noink;
}
var isMac = /Mac/.test(navigator.platform);
var isWindows = /Win/.test(navigator.platform);
var isLinux = /Linux/.test(navigator.userAgent);
var isAndroid = /Android/.test(navigator.userAgent);
var isIOS = /CriOS/.test(navigator.userAgent);
function toDashCase(name) {
  return name.replace(/([a-z0-9])([A-Z])/g, "$1-$2").toLowerCase();
}
var CrLitElement = class extends i4 {
  $;
  willUpdatePending_ = false;
  // Properties for which a '<property-name>-changed' event should be fired
  // whenever they change.
  static notifyProps_ = null;
  constructor() {
    super();
    const self = this;
    this.$ = new Proxy({}, {
      get(cache, id) {
        if (!self.hasUpdated && !self.isConnected) {
          const description = self.tagName + (self.id ? `#${self.id}` : "");
          throw new Error(`CrLitElement ${description} accessed '$.${id}' before connected at least once.`);
        }
        if (!self.hasUpdated) {
          if (self.willUpdatePending_) {
            const description = self.tagName + (self.id ? `#${self.id}` : "");
            throw new Error(`CrLitElement ${description} accessed '$.${id}' within willUpdate().`);
          }
          self.performUpdate();
        }
        if (id in cache) {
          return cache[id];
        }
        const element = self.shadowRoot.querySelector(`#${id}`);
        if (element === null) {
          throw new Error(`CrLitElement ${self.tagName}: Failed to find child with id ${id}`);
        }
        cache[id] = element;
        return element;
      }
    });
  }
  // In a few cases it is necessary to force-render the initial state
  // synchronously instead of waiting for Lit's asynchronous initial render, to
  // make the initial render behavior similar to Polymer, and consequently make
  // migrating from Polymer to Lit easier. Documented known such cases below.
  //
  // Case1: Calling synchronous APIs that access the ShadowDOM.
  // Addressed by the call in connectedCallback().
  //
  // For example CrActionMenuElement provides synchronous APIs showAt(),
  // showAtPosition(), close(), getDialog(), and client code should be able to
  // call these immediately after attaching this element to the DOM, without
  // having to wait for `updateComplete`.
  //
  // Case2: Calling focus() right after a parent dom-if template is stamped.
  // Addressed by CrLitElement's focus() override.
  //
  // This can happen when the following hierarchy is encountered:
  // <dom-if> grandparent > Polymer parent element > Lit child element
  // When the dom-if is stamped, and the parent's connectedCallback() is called,
  // the Lit child's connectedCallback() has not fired yet (unlike Polymer
  // children, which use `_enqueueClient` from [1]), which is problematic
  // when the parent element calls a synchronous API method on the Lit child
  // that assumes that the ShadowDOM is rendered, for example cr-icon-button's
  // focus().
  //
  // [1] https://github.com/Polymer/polymer/blob/1e8b246d01ea99adba305ea04c45d26da31f68f1/lib/mixins/property-effects.js#L1762
  //
  // Case3: Referring to child nodes right after a parent dom-if is stamped.
  // Addressed by the effectively identical logic in the this.$ Proxy above.
  //
  // This happens when the same pattern as Case 2 above is encountered.
  ensureInitialRender() {
    if (!this.hasUpdated) {
      this.performUpdate();
    }
  }
  connectedCallback() {
    super.connectedCallback();
    this.ensureInitialRender();
  }
  willUpdate(_changedProperties) {
    this.willUpdatePending_ = true;
  }
  updated(changedProperties) {
    this.willUpdatePending_ = false;
    const notifyProps = this.constructor.notifyProps_;
    if (notifyProps !== null) {
      const indexableThis = this;
      for (const key of changedProperties.keys()) {
        if (notifyProps.has(key)) {
          if (changedProperties.get(key) === void 0 && indexableThis[key] === void 0) {
            continue;
          }
          this.dispatchEvent(new CustomEvent(
            `${toDashCase(key.toString())}-changed`,
            { detail: { value: indexableThis[key] } }
          ));
        }
      }
    }
  }
  focus(options) {
    this.ensureInitialRender();
    super.focus(options);
  }
  fire(eventName, detail) {
    this.dispatchEvent(
      new CustomEvent(eventName, { bubbles: true, composed: true, detail })
    );
  }
  // Modifies the 'properties' object by:
  //  -  automatically specifying "attribute: <attr_name>" for each reactive
  //     property where attr_name is a dash-case equivalent of the property's
  //     name. For example a 'fooBar' property will be mapped to a 'foo-bar'
  //     attribute, matching Polymer's behavior, instead of Lit's default
  //     behavior (which would map to 'foobar'). This is done to make it easier
  //     to migrate Polymer elements to Lit.
  static patchPropertiesObject() {
    if (!this.hasOwnProperty("properties")) {
      return;
    }
    const properties = this.properties;
    for (const [key, value] of Object.entries(properties)) {
      if (value.attribute == null) {
        value.attribute = toDashCase(key);
      }
    }
    Object.defineProperty(this, "properties", { value: properties });
  }
  static populateNotifyProps() {
    if (!this.hasOwnProperty("properties")) {
      return;
    }
    for (const [key, value] of Object.entries(this.properties)) {
      if (value.notify) {
        if (this.notifyProps_ === null) {
          this.notifyProps_ = /* @__PURE__ */ new Set();
        }
        this.notifyProps_.add(key);
      }
    }
  }
  static finalize() {
    this.patchPropertiesObject();
    this.populateNotifyProps();
    super.finalize();
  }
};
function getCss() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-hairline: 1px solid var(--color-menu-separator,\n      var(--cr-fallback-color-divider));\n  --cr-action-menu-disabled-item-color:\n      var(--color-menu-item-foreground-disabled,\n          var(--cr-fallback-color-disabled-foreground));\n  --cr-action-menu-disabled-item-opacity: 1;\n  --cr-menu-background-color: var(--color-menu-background,\n      var(--cr-fallback-color-surface));\n  --cr-menu-background-focus-color: var(--cr-hover-background-color);\n  --cr-menu-shadow: var(--cr-elevation-2);\n  --cr-primary-text-color: var(--color-menu-item-foreground,\n      var(--cr-fallback-color-on-surface));\n}\n\n:host dialog {\n  background-color: var(--cr-menu-background-color);\n  border: none;\n  border-radius: var(--cr-menu-border-radius, 4px);\n  box-shadow: var(--cr-menu-shadow);\n  margin: 0;\n  min-width: 128px;\n  outline: none;\n  overflow: var(--cr-action-menu-overflow, auto);\n  padding: 0;\n  position: absolute;\n}\n\n/* In unbounded mode, the dialog renders in an external OS window that can\n * extend beyond the host WebContents.\n * 1. 'max-height: none' overrides Blink UA dialog styles that constrain height\n *    to the host document viewport.\n * 2. 'position: fixed' positions the dialog relative to screen/viewport\n *    coordinates rather than the scrolling document layout. */\n:host([use-unbounded]) dialog {\n  max-height: none;\n  position: fixed;\n}\n\n@media (forced-colors: active) {\n  :host dialog {\n    /* Use border instead of box-shadow (which does not work) in Windows\n       HCM. */\n    border: var(--cr-border-hcm);\n  }\n}\n\n:host dialog::backdrop {\n  background-color: transparent;\n}\n\n:host ::slotted(.dropdown-item) {\n  -webkit-tap-highlight-color: transparent;\n  background: none;\n  border: none;\n  border-radius: 0;\n  box-sizing: border-box;\n  color: var(--cr-primary-text-color);\n  font: inherit;\n  min-height: 32px;\n  padding: 8px 24px;\n  text-align: start;\n  user-select: none;\n  width: 100%;\n}\n\n:host ::slotted(.dropdown-item:not([hidden])) {\n  align-items: center;\n  display: flex;\n}\n\n:host ::slotted(.dropdown-item[disabled]) {\n  color: var(--cr-action-menu-disabled-item-color,\n      var(--cr-primary-text-color));\n  opacity: var(--cr-action-menu-disabled-item-opacity, 0.65);\n}\n\n:host ::slotted(.dropdown-item:not([disabled])) {\n  cursor: pointer;\n}\n\n:host ::slotted(.dropdown-item:focus) {\n  background-color: var(--cr-menu-background-focus-color);\n  outline: none;\n}\n\n:host dialog[open] ::slotted(.dropdown-item) {\n  user-select: auto;\n}\n\n:host ::slotted(.dropdown-item:focus-visible) {\n  outline: solid 2px var(--cr-focus-outline-color);\n  outline-offset: -2px;\n}\n\n@media (forced-colors: active) {\n  :host ::slotted(.dropdown-item:focus) {\n    /* Use outline instead of background-color (which does not work) in\n       Windows HCM. */\n    outline: var(--cr-focus-outline-hcm);\n  }\n}\n\n.item-wrapper {\n  outline: none;\n  padding: var(--cr-action-menu-padding, 8px 0);\n}\n"])];
}
function getHtml() {
  return b2`
<dialog id="dialog" part="dialog" @close="${this.onNativeDialogClose_}"
    @beforetoggle="${this.onDialogBeforetoggle_}"
    ?unbounded="${this.useUnbounded}" role="application"
    aria-roledescription="${this.roleDescription || A}">
  <div id="wrapper" class="item-wrapper" role="menu" tabindex="-1"
      aria-label="${this.accessibilityLabel || A}">
    <slot id="contentNode" @slotchange="${this.onSlotchange_}"></slot>
  </div>
</dialog>`;
}
function hasFocusoutOutside(e5, element) {
  return !element || !element.contains(e5.relatedTarget);
}
var DROPDOWN_ITEM_CLASS = "dropdown-item";
var SELECTABLE_DROPDOWN_ITEM_QUERY = `.${DROPDOWN_ITEM_CLASS}:not([hidden]):not([disabled])`;
var AFTER_END_OFFSET = 10;
function getStartPointWithAnchor(start, end, menuLength, anchorAlignment, min, max, unbounded = false) {
  let startPoint = 0;
  switch (anchorAlignment) {
    case -2:
      startPoint = start - menuLength;
      break;
    case -1:
      startPoint = start;
      break;
    case 0:
      startPoint = (start + end - menuLength) / 2;
      break;
    case 1:
      startPoint = end - menuLength;
      break;
    case 2:
      startPoint = end;
      break;
    default:
      assertNotReachedCase(anchorAlignment);
  }
  if (unbounded) {
    const isForward = anchorAlignment === 2 || anchorAlignment === -1;
    const isBackward = anchorAlignment === -2 || anchorAlignment === 1;
    const spaceForward = max - end;
    const spaceBackward = start - min;
    if (isForward && Number.isFinite(max) && startPoint + menuLength > max) {
      if (spaceBackward >= menuLength || spaceBackward > spaceForward) {
        startPoint = anchorAlignment === 2 ? start - menuLength : end - menuLength;
      }
    } else if (isBackward && Number.isFinite(min) && startPoint < min) {
      if (spaceForward >= menuLength || spaceForward > spaceBackward) {
        startPoint = anchorAlignment === -2 ? end : start;
      }
    } else if (anchorAlignment === 0) {
      if (Number.isFinite(max) && startPoint + menuLength > max) {
        startPoint = max - menuLength;
      }
      if (Number.isFinite(min) && startPoint < min) {
        startPoint = min;
      }
    }
    return startPoint;
  }
  if (startPoint + menuLength > max) {
    startPoint = end - menuLength;
  }
  if (startPoint < min) {
    startPoint = start;
  }
  startPoint = Math.max(min, Math.min(startPoint, max - menuLength));
  return startPoint;
}
function getDefaultShowConfig() {
  return {
    top: 0,
    left: 0,
    height: 0,
    width: 0,
    anchorAlignmentX: -1,
    anchorAlignmentY: -1,
    minX: 0,
    minY: 0,
    maxX: 0,
    maxY: 0
  };
}
var CrActionMenuElement = class extends CrLitElement {
  static get is() {
    return "cr-action-menu";
  }
  static get styles() {
    return getCss();
  }
  render() {
    return getHtml.bind(this)();
  }
  static get properties() {
    return {
      // Accessibility text of the menu. Should be something along the lines of
      // "actions", or "more actions".
      accessibilityLabel: { type: String },
      // Setting this flag will make the menu listen for content size changes
      // and reposition to its anchor accordingly.
      autoReposition: { type: Boolean },
      // Setting this flag will cause the menu to automatically close when
      // focus moves outside the menu.
      autoCloseOnFocusout: { type: Boolean },
      open: {
        type: Boolean,
        notify: true
      },
      // Setting this flag will cause the menu to open as a non-modal dialog.
      // Useful when the menu needs to remain open while interacting with
      // other parts of the page.
      nonModal: {
        type: Boolean,
        reflect: true
      },
      // Descriptor of the menu. Should be something along the lines of "menu"
      roleDescription: { type: String },
      // Enables Unbounded Element support on the internal <dialog>, allowing
      // the action menu to render into an external OS surface outside the host
      // window/WebContents. Position calculations use screen coordinates, and
      // the menu does not automatically close on host window resize.
      useUnbounded: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #accessibilityLabel;
  get accessibilityLabel() {
    return this.#accessibilityLabel;
  }
  set accessibilityLabel(_2) {
    this.#accessibilityLabel = _2;
  }
  #autoReposition = false;
  get autoReposition() {
    return this.#autoReposition;
  }
  set autoReposition(_2) {
    this.#autoReposition = _2;
  }
  #autoCloseOnFocusout = false;
  get autoCloseOnFocusout() {
    return this.#autoCloseOnFocusout;
  }
  set autoCloseOnFocusout(_2) {
    this.#autoCloseOnFocusout = _2;
  }
  #open = false;
  get open() {
    return this.#open;
  }
  set open(_2) {
    this.#open = _2;
  }
  #roleDescription;
  get roleDescription() {
    return this.#roleDescription;
  }
  set roleDescription(_2) {
    this.#roleDescription = _2;
  }
  #nonModal = false;
  get nonModal() {
    return this.#nonModal;
  }
  set nonModal(_2) {
    this.#nonModal = _2;
  }
  #useUnbounded = false;
  get useUnbounded() {
    return this.#useUnbounded;
  }
  set useUnbounded(_2) {
    this.#useUnbounded = _2;
  }
  boundClose_ = null;
  resizeObserver_ = null;
  hasMousemoveListener_ = false;
  anchorElement_ = null;
  lastConfig_ = null;
  disconnectedCallback() {
    super.disconnectedCallback();
    this.removeListeners_();
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("useUnbounded") && this.useUnbounded) {
      if (!("showUnboundedElement" in HTMLElement.prototype)) {
        console.warn(
          "CrActionMenu: useUnbounded property is not supported in this environment."
        );
        this.useUnbounded = false;
      }
    }
  }
  firstUpdated() {
    this.addEventListener("keydown", this.onKeyDown_.bind(this));
    this.addEventListener("mouseover", this.onMouseover_);
    this.addEventListener("click", this.onClick_);
    this.addEventListener("focusout", this.onFocusout_.bind(this));
  }
  /**
   * Exposing internal <dialog> elements for tests.
   */
  getDialog() {
    return this.$.dialog;
  }
  getUnboundedDialog_() {
    return this.$.dialog;
  }
  showUnboundedDialog_() {
    this.getUnboundedDialog_().showUnboundedElement().catch(
      (err) => this.handleUnboundedError_("showUnboundedElement", err)
    );
  }
  hideUnboundedDialog_() {
    this.getUnboundedDialog_().hideUnboundedElement().catch(
      (err) => this.handleUnboundedError_("hideUnboundedElement", err)
    );
  }
  handleUnboundedError_(operation, err) {
    if (err instanceof DOMException && err.name === "AbortError") {
      return;
    }
    console.error(`${operation} failed:`, err);
  }
  removeListeners_() {
    window.removeEventListener("resize", this.boundClose_);
    window.removeEventListener("popstate", this.boundClose_);
    if (this.resizeObserver_) {
      this.resizeObserver_.disconnect();
      this.resizeObserver_ = null;
    }
  }
  onFocusout_(e5) {
    if (this.autoCloseOnFocusout && hasFocusoutOutside(e5, this)) {
      this.close();
    }
  }
  onNativeDialogClose_(e5) {
    if (e5.target !== this.$.dialog) {
      return;
    }
    this.fire("close");
  }
  onClick_(e5) {
    if (e5.target === this) {
      this.close();
      e5.stopPropagation();
    }
  }
  onKeyDown_(e5) {
    e5.stopPropagation();
    if (e5.key === "Tab" || e5.key === "Escape") {
      this.close();
      if (e5.key === "Tab") {
        this.fire("tabkeyclose", { shiftKey: e5.shiftKey });
      }
      e5.preventDefault();
      return;
    }
    if ((e5.key !== "Enter" || isMac && e5.ctrlKey) && e5.key !== "ArrowUp" && e5.key !== "ArrowDown") {
      return;
    }
    const options = Array.from(
      this.querySelectorAll(SELECTABLE_DROPDOWN_ITEM_QUERY)
    );
    if (options.length === 0) {
      return;
    }
    const focused = getDeepActiveElement();
    const index = options.findIndex(
      (option) => FocusRow.getFocusableElement(option) === focused
    );
    if (e5.key === "Enter") {
      if (index !== -1) {
        return;
      }
      if (isWindows || isMac) {
        this.close();
        e5.preventDefault();
        return;
      }
    }
    e5.preventDefault();
    this.updateFocus_(options, index, e5.key !== "ArrowUp");
    if (!this.hasMousemoveListener_) {
      this.hasMousemoveListener_ = true;
      this.addEventListener("mousemove", (e22) => {
        this.onMouseover_(e22);
        this.hasMousemoveListener_ = false;
      }, { once: true });
    }
  }
  focusElement_(el) {
    el.focus({ preventScroll: this.useUnbounded });
  }
  onMouseover_(e5) {
    const item = e5.composedPath().find(
      (el) => el.matches && el.matches(SELECTABLE_DROPDOWN_ITEM_QUERY)
    );
    this.focusElement_(item || this.$.wrapper);
  }
  updateFocus_(options, focusedIndex, next) {
    const numOptions = options.length;
    assert(numOptions > 0);
    let index;
    if (focusedIndex === -1) {
      index = next ? 0 : numOptions - 1;
    } else {
      const delta = next ? 1 : -1;
      index = (numOptions + focusedIndex + delta) % numOptions;
    }
    this.focusElement_(options[index]);
  }
  close(unboundedAlreadyDismissed = false) {
    if (!this.open) {
      return;
    }
    this.open = false;
    if (this.useUnbounded && !unboundedAlreadyDismissed) {
      this.hideUnboundedDialog_();
    }
    this.removeListeners_();
    this.$.dialog.close();
    if (this.anchorElement_) {
      assert(this.anchorElement_);
      focusWithoutInk(this.anchorElement_);
      this.anchorElement_ = null;
    }
    if (this.lastConfig_) {
      this.lastConfig_ = null;
    }
  }
  /**
   * Shows the menu anchored to the given element.
   */
  showAt(anchorElement, config) {
    this.anchorElement_ = anchorElement;
    this.anchorElement_.scrollIntoViewIfNeeded();
    const rect = this.anchorElement_.getBoundingClientRect();
    let height = rect.height;
    if (config && !config.noOffset && config.anchorAlignmentY === 2) {
      height -= AFTER_END_OFFSET;
    }
    this.showAtPosition(Object.assign(
      {
        top: rect.top,
        left: rect.left,
        height,
        width: rect.width,
        // Default to anchoring towards the left.
        anchorAlignmentX: 1
        /* BEFORE_END */
      },
      config
    ));
    this.focusElement_(this.$.wrapper);
  }
  /**
   * Shows the menu anchored to the given box. The anchor alignment is
   * specified as an X and Y alignment which represents a point in the anchor
   * where the menu will align to, which can have the menu either before or
   * after the given point in each axis. Center alignment places the center of
   * the menu in line with the center of the anchor. Coordinates are relative to
   * the top-left of the viewport.
   *
   *            y-start
   *         _____________
   *         |           |
   *         |           |
   *         |   CENTER  |
   * x-start |     x     | x-end
   *         |           |
   *         |anchor box |
   *         |___________|
   *
   *             y-end
   *
   * For example, aligning the menu to the inside of the top-right edge of
   * the anchor, extending towards the bottom-left would use a alignment of
   * (BEFORE_END, AFTER_START), whereas centering the menu below the bottom
   * edge of the anchor would use (CENTER, AFTER_END).
   */
  showAtPosition(config) {
    const doc = document.scrollingElement;
    const scrollLeft = doc.scrollLeft;
    const scrollTop = doc.scrollTop;
    this.resetStyle_();
    this.nonModal ? this.$.dialog.show() : this.$.dialog.showModal();
    this.open = true;
    if (this.useUnbounded) {
      this.$.dialog.toggleAttribute("unbounded", true);
      this.positionDialog_(config);
      this.showUnboundedDialog_();
    } else {
      config.top += scrollTop;
      config.left += scrollLeft;
      this.positionDialog_(Object.assign(
        {
          minX: scrollLeft,
          minY: scrollTop,
          maxX: scrollLeft + doc.clientWidth,
          maxY: scrollTop + doc.clientHeight
        },
        config
      ));
    }
    doc.scrollTop = scrollTop;
    doc.scrollLeft = scrollLeft;
    this.addListeners_();
    const openedByKey = FocusOutlineManager.forDocument(document).visible;
    if (openedByKey) {
      const firstSelectableItem = this.querySelector(SELECTABLE_DROPDOWN_ITEM_QUERY);
      if (firstSelectableItem) {
        requestAnimationFrame(() => {
          this.focusElement_(firstSelectableItem);
        });
      }
    }
  }
  resetStyle_() {
    this.$.dialog.style.left = "";
    this.$.dialog.style.right = "";
    this.$.dialog.style.top = "0";
  }
  /**
   * Position the dialog using the coordinates in config. Coordinates are
   * relative to the top-left of the viewport when scrolled to (0, 0).
   */
  positionDialog_(config) {
    this.lastConfig_ = config;
    const c5 = Object.assign(getDefaultShowConfig(), config);
    if (this.useUnbounded) {
      const screenLeft = window.screenX;
      const screenTop = window.screenY;
      const screenWidth = window.screen.availWidth;
      const screenHeight = window.screen.availHeight;
      if (config.minX === void 0) {
        c5.minX = -screenLeft;
      }
      if (config.minY === void 0) {
        c5.minY = -screenTop;
      }
      if (config.maxX === void 0) {
        c5.maxX = screenWidth - screenLeft;
      }
      if (config.maxY === void 0) {
        c5.maxY = screenHeight - screenTop;
      }
    }
    const top = c5.top;
    const left = c5.left;
    const bottom = top + c5.height;
    const right = left + c5.width;
    const rtl = getComputedStyle(this).direction === "rtl";
    if (rtl) {
      c5.anchorAlignmentX *= -1;
    }
    const offsetWidth = this.$.dialog.offsetWidth;
    const menuLeft = getStartPointWithAnchor(
      left,
      right,
      offsetWidth,
      c5.anchorAlignmentX,
      c5.minX,
      c5.maxX,
      this.useUnbounded
    );
    if (rtl) {
      const menuRight = document.scrollingElement.clientWidth - menuLeft - offsetWidth;
      this.$.dialog.style.right = menuRight + "px";
    } else {
      this.$.dialog.style.left = menuLeft + "px";
    }
    const menuTop = getStartPointWithAnchor(
      top,
      bottom,
      this.$.dialog.offsetHeight,
      c5.anchorAlignmentY,
      c5.minY,
      c5.maxY,
      this.useUnbounded
    );
    this.$.dialog.style.top = menuTop + "px";
  }
  onSlotchange_() {
    for (const node of this.$.contentNode.assignedElements({ flatten: true })) {
      if (node.classList.contains(DROPDOWN_ITEM_CLASS) && !node.getAttribute("role")) {
        node.setAttribute("role", "menuitem");
      }
    }
  }
  /**
   * In unbounded mode, the native OS popup window can be dismissed directly by
   * light dismiss (e.g. clicking outside the host document), firing a
   * Blink 'beforetoggle' ToggleEvent when its presentation state transitions to
   * 'closed'. We listen for this event to close the dialog when the unbounded
   * window gets light dismissed.
   */
  onDialogBeforetoggle_(e5) {
    if (!this.useUnbounded) {
      return;
    }
    const toggleEvent = e5;
    if (toggleEvent.newState === "closed" && this.open) {
      this.close(
        /*unboundedAlreadyDismissed=*/
        true
      );
    }
  }
  addListeners_() {
    this.boundClose_ = this.boundClose_ || (() => {
      if (this.$.dialog.open) {
        this.close();
      }
    });
    window.addEventListener("resize", this.boundClose_);
    window.addEventListener("popstate", this.boundClose_);
    if (this.autoReposition) {
      this.resizeObserver_ = new ResizeObserver(() => {
        if (this.lastConfig_) {
          this.positionDialog_(this.lastConfig_);
          this.fire("cr-action-menu-repositioned");
        }
      });
      this.resizeObserver_.observe(this.$.dialog);
    }
  }
};
customElements.define(CrActionMenuElement.is, CrActionMenuElement);
function getCss2() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  bottom: 0;\n  display: block;\n  left: 0;\n  overflow: hidden;\n  pointer-events: none;\n  position: absolute;\n  right: 0;\n  top: 0;\n  /* For rounded corners: http://jsbin.com/temexa/4. */\n  transform: translate3d(0, 0, 0);\n}\n\n.ripple {\n  background-color: currentcolor;\n  left: 0;\n  opacity: var(--paper-ripple-opacity, 0.25);\n  pointer-events: none;\n  position: absolute;\n  will-change: height, transform, width;\n}\n\n.ripple,\n:host(.circle) {\n  border-radius: 50%;\n}\n"])];
}
var MAX_RADIUS_PX = 300;
var MIN_DURATION_MS = 800;
function distance(x1, y1, x2, y22) {
  const xDelta = x1 - x2;
  const yDelta = y1 - y22;
  return Math.sqrt(xDelta * xDelta + yDelta * yDelta);
}
var CrRippleElement = class extends CrLitElement {
  static get is() {
    return "cr-ripple";
  }
  static get styles() {
    return getCss2();
  }
  static get properties() {
    return {
      holdDown: { type: Boolean },
      recenters: { type: Boolean },
      noink: { type: Boolean }
    };
  }
  #holdDown = false;
  get holdDown() {
    return this.#holdDown;
  }
  set holdDown(_2) {
    this.#holdDown = _2;
  }
  #recenters = false;
  get recenters() {
    return this.#recenters;
  }
  set recenters(_2) {
    this.#recenters = _2;
  }
  #noink = false;
  get noink() {
    return this.#noink;
  }
  set noink(_2) {
    this.#noink = _2;
  }
  ripples_ = [];
  eventTracker_ = new EventTracker();
  connectedCallback() {
    super.connectedCallback();
    assert(this.parentNode);
    const keyEventTarget = this.parentNode.nodeType === Node.DOCUMENT_FRAGMENT_NODE ? this.parentNode.host : this.parentElement;
    this.eventTracker_.add(
      keyEventTarget,
      "pointerdown",
      (e5) => this.uiDownAction(e5)
    );
    this.eventTracker_.add(
      keyEventTarget,
      "pointerup",
      () => this.uiUpAction()
    );
    this.eventTracker_.add(
      keyEventTarget,
      "pointerout",
      () => this.uiUpAction()
    );
    this.eventTracker_.add(keyEventTarget, "keydown", (e5) => {
      if (e5.defaultPrevented) {
        return;
      }
      if (e5.key === "Enter") {
        this.onEnterKeydown_();
        return;
      }
      if (e5.key === " ") {
        this.onSpaceKeydown_();
      }
    });
    this.eventTracker_.add(keyEventTarget, "keyup", (e5) => {
      if (e5.defaultPrevented) {
        return;
      }
      if (e5.key === " ") {
        this.onSpaceKeyup_();
      }
    });
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    this.eventTracker_.removeAll();
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("holdDown")) {
      this.holdDownChanged_(this.holdDown, changedProperties.get("holdDown"));
    }
  }
  uiDownAction(e5) {
    if (e5 !== void 0 && e5.button !== 0) {
      return;
    }
    if (!this.noink) {
      this.downAction_(e5);
    }
  }
  downAction_(e5) {
    if (this.ripples_.length && this.holdDown) {
      return;
    }
    this.showRipple_(e5);
  }
  clear() {
    this.hideRipple_();
    this.holdDown = false;
  }
  showAndHoldDown() {
    this.ripples_.forEach((ripple) => {
      ripple.remove();
    });
    this.ripples_ = [];
    this.holdDown = true;
  }
  showRipple_(e5) {
    const rect = this.getBoundingClientRect();
    const roundedCenterX = function() {
      return Math.round(rect.width / 2);
    };
    const roundedCenterY = function() {
      return Math.round(rect.height / 2);
    };
    let x2 = 0;
    let y3 = 0;
    const centered = !e5;
    if (centered) {
      x2 = roundedCenterX();
      y3 = roundedCenterY();
    } else {
      x2 = Math.round(e5.clientX - rect.left);
      y3 = Math.round(e5.clientY - rect.top);
    }
    const corners = [
      { x: 0, y: 0 },
      { x: rect.width, y: 0 },
      { x: 0, y: rect.height },
      { x: rect.width, y: rect.height }
    ];
    const cornerDistances = corners.map(function(corner) {
      return Math.round(distance(x2, y3, corner.x, corner.y));
    });
    const radius = Math.min(MAX_RADIUS_PX, Math.max.apply(Math, cornerDistances));
    const startTranslate = `${x2 - radius}px, ${y3 - radius}px`;
    let endTranslate = startTranslate;
    if (this.recenters && !centered) {
      endTranslate = `${roundedCenterX() - radius}px, ${roundedCenterY() - radius}px`;
    }
    const ripple = document.createElement("div");
    ripple.classList.add("ripple");
    ripple.style.height = ripple.style.width = 2 * radius + "px";
    this.ripples_.push(ripple);
    this.shadowRoot.appendChild(ripple);
    ripple.animate(
      {
        transform: [
          `translate(${startTranslate}) scale(0)`,
          `translate(${endTranslate}) scale(1)`
        ]
      },
      {
        duration: Math.max(MIN_DURATION_MS, Math.log(radius) * radius) || 0,
        easing: "cubic-bezier(.2, .9, .1, .9)",
        fill: "forwards"
      }
    );
  }
  uiUpAction() {
    if (!this.noink) {
      this.upAction_();
    }
  }
  upAction_() {
    if (!this.holdDown) {
      this.hideRipple_();
    }
  }
  hideRipple_() {
    if (this.ripples_.length === 0) {
      return;
    }
    this.ripples_.forEach(function(ripple) {
      const opacity = ripple.computedStyleMap().get("opacity") ?? null;
      if (opacity === null) {
        ripple.remove();
        return;
      }
      const animation = ripple.animate(
        {
          opacity: [opacity.value, 0]
        },
        {
          duration: 150,
          fill: "forwards"
        }
      );
      animation.finished.then(() => {
        ripple.remove();
      });
    });
    this.ripples_ = [];
  }
  onEnterKeydown_() {
    this.uiDownAction();
    window.setTimeout(() => {
      this.uiUpAction();
    }, 1);
  }
  onSpaceKeydown_() {
    this.uiDownAction();
  }
  onSpaceKeyup_() {
    this.uiUpAction();
  }
  holdDownChanged_(newHoldDown, oldHoldDown) {
    if (oldHoldDown === void 0) {
      return;
    }
    if (newHoldDown) {
      this.downAction_();
    } else {
      this.upAction_();
    }
  }
};
customElements.define(CrRippleElement.is, CrRippleElement);
var CrRippleMixin = (superClass) => {
  class CrRippleMixin2 extends superClass {
    static get properties() {
      return {
        /**
         * If true, the element will not produce a ripple effect when
         * interacted with via the pointer.
         */
        noink: { type: Boolean }
      };
    }
    #noink = false;
    get noink() {
      return this.#noink;
    }
    set noink(_2) {
      this.#noink = _2;
    }
    rippleContainer = null;
    ripple_ = null;
    updated(changedProperties) {
      super.updated(changedProperties);
      if (changedProperties.has("noink") && this.hasRipple()) {
        assert(this.ripple_);
        this.ripple_.noink = this.noink;
      }
    }
    ensureRippleOnPointerdown() {
      this.addEventListener(
        "pointerdown",
        () => this.ensureRipple(),
        { capture: true }
      );
    }
    /**
     * Ensures this element contains a ripple effect. For startup efficiency
     * the ripple effect is dynamically added on demand when needed.
     */
    ensureRipple() {
      if (this.hasRipple()) {
        return;
      }
      this.ripple_ = this.createRipple();
      this.ripple_.noink = this.noink;
      const rippleContainer = this.rippleContainer || this.shadowRoot;
      assert(rippleContainer);
      rippleContainer.appendChild(this.ripple_);
    }
    /**
     * Returns the `<cr-ripple>` element used by this element to create
     * ripple effects. The element's ripple is created on demand, when
     * necessary, and calling this method will force the
     * ripple to be created.
     */
    getRipple() {
      this.ensureRipple();
      assert(this.ripple_);
      return this.ripple_;
    }
    /**
     * Returns true if this element currently contains a ripple effect.
     */
    hasRipple() {
      return Boolean(this.ripple_);
    }
    /**
     * Create the element's ripple effect via creating a `<cr-ripple
     * id="ink">` instance. Override this method to customize the ripple
     * element.
     */
    createRipple() {
      const ripple = document.createElement("cr-ripple");
      ripple.id = "ink";
      return ripple;
    }
  }
  return CrRippleMixin2;
};
function getCss3() {
  return [i([`/* Copyright 2024 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #scheme=relative
 * #css_wrapper_metadata_end */

/* Included here so we don't have to include "iron-positioning" in every
 * stylesheet. See crbug.com/498405. */
[hidden],
:host([hidden]) {
  display: none !important;
}
`])];
}
function getCss4() {
  return [getCss3(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_hidden_style_lit.css.js\n * #import=../cr_shared_vars.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-button-background-color: transparent;\n  --cr-button-border-color:  var(--color-button-border,\n      var(--cr-fallback-color-tonal-outline));\n  --cr-button-text-color: var(--color-button-foreground,\n      var(--cr-fallback-color-primary));\n  --cr-button-ripple-opacity: 1;\n  --cr-button-ripple-color: var(--cr-active-background-color);\n\n  --cr-button-disabled-background-color: transparent;\n  --cr-button-disabled-border-color: var(--color-button-border-disabled,\n      var(--cr-fallback-color-disabled-background));\n  --cr-button-disabled-text-color: var(--color-button-foreground-disabled,\n      var(--cr-fallback-color-disabled-foreground));\n\n  flex-shrink: 0;\n  display: inline-flex;\n  align-items: center;\n  justify-content: center;\n  box-sizing: border-box;\n  min-width: 5.14em;\n  height: var(--cr-button-height);\n  padding: 8px 16px;\n  outline-width: 0;\n  overflow: hidden;\n  position: relative;\n  cursor: pointer;\n  user-select: none;\n  -webkit-tap-highlight-color: transparent;\n  border: var(--cr-button-border, 1px solid var(--cr-button-border-color));\n  border-radius: 100px;\n  background: var(--cr-button-background-color);\n  color: var(--cr-button-text-color);\n  font-weight: 500;\n  line-height: 20px;\n  isolation: isolate;\n}\n\n:host(.action-button) {\n  --cr-button-background-color: var(--color-button-background-prominent,\n      var(--cr-fallback-color-primary));\n  --cr-button-text-color: var(--color-button-foreground-prominent,\n      var(--cr-fallback-color-on-primary));\n  --cr-button-ripple-color: var(--cr-active-on-primary-background-color);\n  --cr-button-border: none;\n\n  --cr-button-disabled-background-color: var(\n      --color-button-background-prominent-disabled,\n      var(--cr-fallback-color-disabled-background));\n  --cr-button-disabled-text-color: var(--color-button-foreground-disabled,\n      var(--cr-fallback-color-disabled-foreground));\n  --cr-button-disabled-border: none;\n}\n\n:host(.tonal-button),\n:host(.floating-button) {\n  --cr-button-background-color: var(--color-button-background-tonal,\n      var(--cr-fallback-color-secondary-container));\n  --cr-button-text-color: var(--color-button-foreground-tonal,\n      var(--cr-fallback-color-on-tonal-container));\n  --cr-button-border: none;\n\n  --cr-button-disabled-background-color: var(\n      --color-button-background-tonal-disabled,\n      var(--cr-fallback-color-disabled-background));\n  --cr-button-disabled-text-color: var(--color-button-foreground-disabled,\n      var(--cr-fallback-color-disabled-foreground));\n  --cr-button-disabled-border: none;\n}\n\n@media (forced-colors: active) {\n  @container style(--color-button-foreground) {\n    :host {\n      /* Color pipeline colors automatically provide high-contrast colors in\n      * high-contrast mode, and fallbackvars have high-contrast fallbacks\n      * for critically important states such as disabled state. */\n      forced-color-adjust: none;\n    }\n  }\n}\n\n:host(.floating-button) {\n  border-radius: 8px;\n  height: 40px;\n  transition: box-shadow 80ms linear;\n}\n\n:host(.floating-button:hover) {\n  box-shadow: var(--cr-elevation-3);\n}\n\n:host([has-prefix-icon_]),\n:host([has-suffix-icon_]) {\n  --iron-icon-height: 20px;\n  --iron-icon-width: 20px;\n  --icon-block-padding-large: 16px;\n  --icon-block-padding-small: 12px;\n  gap: 8px;\n  padding-block-end: 8px;\n  padding-block-start: 8px;\n}\n\n:host([has-prefix-icon_]) {\n  padding-inline-end: var(--icon-block-padding-large);\n  padding-inline-start: var(--icon-block-padding-small);\n}\n\n:host([has-suffix-icon_]) {\n  padding-inline-end: var(--icon-block-padding-small);\n  padding-inline-start: var(--icon-block-padding-large);\n}\n\n:host-context(.focus-outline-visible):host(:focus) {\n  box-shadow: none;\n  outline: 2px solid var(--cr-focus-outline-color);\n  outline-offset: 2px;\n}\n\n#background {\n  border-radius: inherit;\n  inset: 0;\n  pointer-events: none;\n  position: absolute;\n  z-index: 0;\n}\n\n#content {\n  display: inline;\n}\n\n#hoverBackground {\n  content: '';\n  display: none;\n  inset: 0;\n  pointer-events: none;\n  position: absolute;\n  z-index: 1;\n}\n\n:host(:hover) #hoverBackground {\n  background: var(--cr-hover-background-color);\n  display: block;\n}\n\n:host(.action-button:hover) #hoverBackground {\n  background: var(--cr-hover-on-prominent-background-color);\n}\n\n:host([disabled]) {\n  background: var(--cr-button-disabled-background-color);\n  border: var(--cr-button-disabled-border,\n      1px solid var(--cr-button-disabled-border-color));\n  color: var(--cr-button-disabled-text-color);\n  cursor: auto;\n  pointer-events: none;\n}\n\n/* cancel-button is meant to be used within a cr-dialog */\n:host(.cancel-button) {\n  margin-inline-end: 8px;\n}\n\n:host(.action-button),\n:host(.cancel-button) {\n  line-height: 154%;\n}\n\n#ink {\n  color: var(--cr-button-ripple-color);\n  --paper-ripple-opacity: var(--cr-button-ripple-opacity);\n}\n\n/* Layering */\n#hoverBackground,\ncr-ripple { z-index: 1; }\n\n#content,\n::slotted(*) { z-index: 2; }\n"])];
}
function getHtml2() {
  return b2`
<div id="background"></div>
<slot id="prefixIcon" name="prefix-icon"
    @slotchange="${this.onPrefixIconSlotchange_}">
</slot>
<span id="content">
  <slot></slot>
</span>
<slot id="suffixIcon" name="suffix-icon"
    @slotchange="${this.onSuffixIconSlotchange_}">
</slot>
<div id="hoverBackground" part="hoverBackground"></div>`;
}
var CrButtonElementBase = CrRippleMixin(CrLitElement);
var CrButtonElement = class extends CrButtonElementBase {
  static get is() {
    return "cr-button";
  }
  static get styles() {
    return getCss4();
  }
  render() {
    return getHtml2.bind(this)();
  }
  static get properties() {
    return {
      disabled: {
        type: Boolean,
        reflect: true
      },
      hasPrefixIcon_: {
        type: Boolean,
        reflect: true
      },
      hasSuffixIcon_: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #disabled = false;
  get disabled() {
    return this.#disabled;
  }
  set disabled(_2) {
    this.#disabled = _2;
  }
  #hasPrefixIcon_ = false;
  get hasPrefixIcon_() {
    return this.#hasPrefixIcon_;
  }
  set hasPrefixIcon_(_2) {
    this.#hasPrefixIcon_ = _2;
  }
  #hasSuffixIcon_ = false;
  get hasSuffixIcon_() {
    return this.#hasSuffixIcon_;
  }
  set hasSuffixIcon_(_2) {
    this.#hasSuffixIcon_ = _2;
  }
  /**
   * It is possible to activate a tab when the space key is pressed down. When
   * this element has focus, the keyup event for the space key should not
   * perform a 'click'. |spaceKeyDown_| tracks when a space pressed and
   * handled by this element. Space keyup will only result in a 'click' when
   * |spaceKeyDown_| is true. |spaceKeyDown_| is set to false when element
   * loses focus.
   */
  spaceKeyDown_ = false;
  timeoutIds_ = /* @__PURE__ */ new Set();
  constructor() {
    super();
    this.addEventListener("blur", this.onBlur_.bind(this));
    this.addEventListener("click", this.onClick_.bind(this));
    this.addEventListener("keydown", this.onKeyDown_.bind(this));
    this.addEventListener("keyup", this.onKeyUp_.bind(this));
    this.ensureRippleOnPointerdown();
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    this.timeoutIds_.forEach(clearTimeout);
    this.timeoutIds_.clear();
  }
  firstUpdated() {
    if (!this.hasAttribute("role")) {
      this.setAttribute("role", "button");
    }
    if (!this.hasAttribute("tabindex")) {
      this.setAttribute("tabindex", "0");
    }
    FocusOutlineManager.forDocument(document);
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("disabled")) {
      this.setAttribute("aria-disabled", this.disabled ? "true" : "false");
      this.disabledChanged_(this.disabled, changedProperties.get("disabled"));
    }
  }
  setTimeout_(fn, delay) {
    if (!this.isConnected) {
      return;
    }
    const id = setTimeout(() => {
      this.timeoutIds_.delete(id);
      fn();
    }, delay);
    this.timeoutIds_.add(id);
  }
  disabledChanged_(newValue, oldValue) {
    if (!newValue && oldValue === void 0) {
      return;
    }
    if (this.disabled) {
      this.blur();
    }
    this.setAttribute("tabindex", String(this.disabled ? -1 : 0));
  }
  onBlur_() {
    this.spaceKeyDown_ = false;
    this.setTimeout_(() => this.getRipple().uiUpAction(), 100);
  }
  onClick_(e5) {
    if (this.disabled) {
      e5.stopImmediatePropagation();
    }
  }
  onPrefixIconSlotchange_() {
    this.hasPrefixIcon_ = this.$.prefixIcon.assignedElements().length > 0;
  }
  onSuffixIconSlotchange_() {
    this.hasSuffixIcon_ = this.$.suffixIcon.assignedElements().length > 0;
  }
  onKeyDown_(e5) {
    if (e5.key !== " " && (e5.key !== "Enter" || isMac && e5.ctrlKey)) {
      return;
    }
    e5.preventDefault();
    e5.stopPropagation();
    if (e5.repeat) {
      return;
    }
    this.getRipple().uiDownAction();
    if (e5.key === "Enter") {
      this.click();
      this.setTimeout_(() => this.getRipple().uiUpAction(), 100);
    } else if (e5.key === " ") {
      this.spaceKeyDown_ = true;
    }
  }
  onKeyUp_(e5) {
    if (e5.key !== " " && (e5.key !== "Enter" || isMac && e5.ctrlKey)) {
      return;
    }
    e5.preventDefault();
    e5.stopPropagation();
    if (this.spaceKeyDown_ && e5.key === " ") {
      this.spaceKeyDown_ = false;
      this.click();
      this.getRipple().uiUpAction();
    }
  }
};
customElements.define(CrButtonElement.is, CrButtonElement);
function getCss5() {
  return [getCss3(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_hidden_style_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  align-items: center;\n  display: inline-flex;\n  justify-content: center;\n  position: relative;\n\n  vertical-align: middle;\n\n  /* TODO (rbpotter): Change variable names after migration. */\n  fill: var(--iron-icon-fill-color, currentcolor);\n  stroke: var(--iron-icon-stroke-color, none);\n\n  width: var(--iron-icon-width, 24px);\n  height: var(--iron-icon-height, 24px);\n}\n\n:host-context([webui-refresh-2026]) {\n  &:host(:focus) {\n    border-radius: 2px;\n    outline: solid 2px var(--cr-focus-outline-color);\n  }\n}\n"])];
}
var iconsetMap = null;
var IconsetMap = class _IconsetMap extends EventTarget {
  iconsets_ = /* @__PURE__ */ new Map();
  static getInstance() {
    return iconsetMap || (iconsetMap = new _IconsetMap());
  }
  static resetInstanceForTesting(instance2) {
    iconsetMap = instance2;
  }
  get(id) {
    return this.iconsets_.get(id) || null;
  }
  set(id, iconset) {
    assert(
      !this.iconsets_.has(id),
      `Tried to add a second iconset with id '${id}'`
    );
    this.iconsets_.set(id, iconset);
    this.dispatchEvent(new CustomEvent("cr-iconset-added", { detail: id }));
  }
};
var CrIconElement = class extends CrLitElement {
  static get is() {
    return "cr-icon";
  }
  static get styles() {
    return getCss5();
  }
  static get properties() {
    return {
      /**
       * The name of the icon to use. The name should be of the form:
       * `iconset_name:icon_name`.
       */
      icon: { type: String }
    };
  }
  #icon = "";
  get icon() {
    return this.#icon;
  }
  set icon(_2) {
    this.#icon = _2;
  }
  iconsetName_ = "";
  iconName_ = "";
  iconset_ = null;
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("icon")) {
      const [iconsetName, iconName] = this.icon.split(":");
      this.iconName_ = iconName || "";
      this.iconsetName_ = iconsetName || "";
      this.updateIcon_();
    }
  }
  updateIcon_() {
    if (this.iconName_ === "" && this.iconset_) {
      this.iconset_.removeIcon(this);
    } else if (this.iconsetName_) {
      const iconsetMap2 = IconsetMap.getInstance();
      this.iconset_ = iconsetMap2.get(this.iconsetName_);
      assert(
        this.iconset_,
        `Could not find iconset for: '${this.iconsetName_}:${this.iconName_}'`
      );
      this.iconset_.applyIcon(this, this.iconName_);
    }
  }
};
customElements.define(CrIconElement.is, CrIconElement);
function getCss6() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_shared_vars.css.js\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-icon-button-fill-color: currentColor;\n  --cr-icon-button-icon-start-offset: 0;\n  --cr-icon-button-icon-size: 20px;\n  --cr-icon-button-size: 32px;\n  --cr-icon-button-height: var(--cr-icon-button-size);\n  --cr-icon-button-transition: 150ms ease-in-out;\n  --cr-icon-button-width: var(--cr-icon-button-size);\n  /* Copied from paper-fab.html. Prevents square touch highlight. */\n  -webkit-tap-highlight-color: transparent;\n  border-radius: 50%;\n  color: var(--cr-icon-button-stroke-color,\n      var(--cr-icon-button-fill-color));\n  cursor: pointer;\n  display: inline-flex;\n  flex-shrink: 0;\n  height: var(--cr-icon-button-height);\n  margin-inline-end: var(--cr-icon-button-margin-end,\n      var(--cr-icon-ripple-margin));\n  margin-inline-start: var(--cr-icon-button-margin-start);\n  outline: none;\n  overflow: hidden;\n  pointer-events: auto;\n  position: relative;\n  user-select: none;\n  vertical-align: middle;\n  width: var(--cr-icon-button-width);\n}\n\n:host(:hover:not([disabled])) {\n  background-color: var(--cr-icon-button-hover-background-color,\n      var(--cr-hover-background-color));\n}\n\n:host(:focus-visible:focus) {\n  box-shadow: inset 0 0 0 2px var(--cr-icon-button-focus-outline-color,\n      var(--cr-focus-outline-color));\n}\n\n@media (forced-colors: active) {\n  :host(:focus-visible:focus) {\n    /* Use outline instead of box-shadow (which does not work) in Windows\n       HCM. */\n    outline: var(--cr-focus-outline-hcm);\n  }\n}\n\n#ink {\n  --paper-ripple-opacity: 1;\n  color: var(--cr-icon-button-active-background-color,\n      var(--cr-active-background-color));\n}\n\n:host([disabled]) {\n  cursor: initial;\n  opacity: var(--cr-disabled-opacity);\n  pointer-events: none;\n}\n\n:host(.no-overlap) {\n  --cr-icon-button-margin-end: 0;\n  --cr-icon-button-margin-start: 0;\n}\n\n:host-context([dir=rtl]):host(\n    :not([suppress-rtl-flip]):not([multiple-icons_])) {\n  transform: scaleX(-1);  /* Invert X: flip on the Y axis (aka mirror). */\n}\n\n:host-context([dir=rtl]):host(\n    :not([suppress-rtl-flip])[multiple-icons_]) cr-icon {\n  transform: scaleX(-1);  /* Invert X: flip on the Y axis (aka mirror). */\n}\n\n:host(:not([iron-icon])) #maskedImage {\n  -webkit-mask-image: var(--cr-icon-image);\n  -webkit-mask-position: center;\n  -webkit-mask-repeat: no-repeat;\n  -webkit-mask-size: var(--cr-icon-button-icon-size);\n  -webkit-transform: var(--cr-icon-image-transform, none);\n  background-color: var(--cr-icon-button-fill-color);\n  height: 100%;\n  transition: background-color var(--cr-icon-button-transition);\n  width: 100%;\n}\n\n@media (forced-colors: active) {\n  :host(:not([iron-icon])) #maskedImage {\n    background-color: ButtonText;\n  }\n}\n\n#icon {\n  align-items: center;\n  border-radius: 4px;\n  display: flex;\n  height: 100%;\n  justify-content: center;\n  padding-inline-start: var(--cr-icon-button-icon-start-offset);\n  pointer-events: none;\n  /* The |_rippleContainer| must be position relative. */\n  position: relative;\n  width: 100%;\n}\n\ncr-icon {\n  --iron-icon-fill-color: var(--cr-icon-button-fill-color);\n  --iron-icon-stroke-color: var(--cr-icon-button-stroke-color, none);\n  --iron-icon-height: var(--cr-icon-button-icon-size);\n  --iron-icon-width: var(--cr-icon-button-icon-size);\n  transition: fill var(--cr-icon-button-transition),\n      stroke var(--cr-icon-button-transition);\n}\n\n@media (prefers-color-scheme: dark) {\n  :host {\n    --cr-icon-button-fill-color: var(--google-grey-500);\n  }\n}\n"])];
}
function getHtml3() {
  return b2`
<div id="icon">
  <div id="maskedImage"></div>
</div>`;
}
var CrIconbuttonElementBase = CrRippleMixin(CrLitElement);
var CrIconButtonElement = class extends CrIconbuttonElementBase {
  static get is() {
    return "cr-icon-button";
  }
  static get styles() {
    return getCss6();
  }
  render() {
    return getHtml3.bind(this)();
  }
  static get properties() {
    return {
      disabled: {
        type: Boolean,
        reflect: true
      },
      ironIcon: {
        type: String,
        reflect: true
      },
      suppressRtlFlip: {
        type: Boolean,
        value: false,
        reflect: true
      },
      multipleIcons_: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #disabled = false;
  get disabled() {
    return this.#disabled;
  }
  set disabled(_2) {
    this.#disabled = _2;
  }
  #ironIcon;
  get ironIcon() {
    return this.#ironIcon;
  }
  set ironIcon(_2) {
    this.#ironIcon = _2;
  }
  #suppressRtlFlip = false;
  get suppressRtlFlip() {
    return this.#suppressRtlFlip;
  }
  set suppressRtlFlip(_2) {
    this.#suppressRtlFlip = _2;
  }
  #multipleIcons_ = false;
  get multipleIcons_() {
    return this.#multipleIcons_;
  }
  set multipleIcons_(_2) {
    this.#multipleIcons_ = _2;
  }
  /**
   * It is possible to activate a tab when the space key is pressed down. When
   * this element has focus, the keyup event for the space key should not
   * perform a 'click'. |spaceKeyDown_| tracks when a space pressed and
   * handled by this element. Space keyup will only result in a 'click' when
   * |spaceKeyDown_| is true. |spaceKeyDown_| is set to false when element
   * loses focus.
   */
  spaceKeyDown_ = false;
  constructor() {
    super();
    this.addEventListener("blur", this.onBlur_.bind(this));
    this.addEventListener("click", this.onClick_.bind(this));
    this.addEventListener("keydown", this.onKeyDown_.bind(this));
    this.addEventListener("keyup", this.onKeyUp_.bind(this));
    this.ensureRippleOnPointerdown();
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("ironIcon")) {
      const icons2 = (this.ironIcon || "").split(",");
      this.multipleIcons_ = icons2.length > 1;
    }
  }
  firstUpdated() {
    if (!this.hasAttribute("role")) {
      this.setAttribute("role", "button");
    }
    if (!this.hasAttribute("tabindex")) {
      this.setAttribute("tabindex", "0");
    }
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("disabled")) {
      this.setAttribute("aria-disabled", this.disabled ? "true" : "false");
      this.disabledChanged_(this.disabled, changedProperties.get("disabled"));
    }
    if (changedProperties.has("ironIcon")) {
      this.onIronIconChanged_();
    }
  }
  disabledChanged_(newValue, oldValue) {
    if (!newValue && oldValue === void 0) {
      return;
    }
    if (this.disabled) {
      this.blur();
    }
    this.setAttribute("tabindex", String(this.disabled ? -1 : 0));
  }
  onBlur_() {
    this.spaceKeyDown_ = false;
  }
  onClick_(e5) {
    if (this.disabled) {
      e5.stopImmediatePropagation();
    }
  }
  onIronIconChanged_() {
    this.shadowRoot.querySelectorAll("cr-icon").forEach((el) => el.remove());
    if (!this.ironIcon) {
      return;
    }
    const icons2 = (this.ironIcon || "").split(",");
    icons2.forEach(async (icon) => {
      const crIcon = document.createElement("cr-icon");
      crIcon.icon = icon;
      crIcon.setAttribute("part", "icon");
      this.$.icon.appendChild(crIcon);
      await crIcon.updateComplete;
      crIcon.shadowRoot.querySelectorAll("svg, img").forEach((child) => child.setAttribute("role", "none"));
    });
  }
  onKeyDown_(e5) {
    if (e5.key !== " " && (e5.key !== "Enter" || isMac && e5.ctrlKey)) {
      return;
    }
    e5.preventDefault();
    e5.stopPropagation();
    if (e5.repeat) {
      return;
    }
    if (e5.key === "Enter") {
      this.click();
    } else if (e5.key === " ") {
      this.spaceKeyDown_ = true;
    }
  }
  onKeyUp_(e5) {
    if (e5.key === " " || e5.key === "Enter" && !(isMac && e5.ctrlKey)) {
      e5.preventDefault();
      e5.stopPropagation();
    }
    if (this.spaceKeyDown_ && e5.key === " ") {
      this.spaceKeyDown_ = false;
      this.click();
    }
  }
};
customElements.define(CrIconButtonElement.is, CrIconButtonElement);
function getCss7() {
  return [i(['/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n.icon-arrow-back {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_back_old.svg");\n}\n\n.icon-arrow-dropdown {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_dropdown_old.svg");\n}\n\n.icon-arrow-drop-down-cr23 {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_drop_down_cr23_old.svg");\n}\n\n.icon-arrow-drop-up-cr23 {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_drop_up_cr23_old.svg");\n}\n\n.icon-arrow-upward {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_upward_old.svg");\n}\n\n.icon-arrow-forward {\n  --cr-icon-image: url("/newtab/chromium/images/icon_arrow_forward_old.svg");\n}\n\n.icon-cancel {\n  --cr-icon-image: url("/newtab/chromium/images/icon_cancel_old.svg");\n}\n\n.icon-clear {\n  --cr-icon-image: url("/newtab/chromium/images/icon_clear_old.svg");\n}\n\n.icon-copy-content {\n  --cr-icon-image: url("/newtab/chromium/images/icon_copy_content_old.svg");\n}\n\n.icon-delete-gray {\n  --cr-icon-image: url("/newtab/chromium/images/icon_delete_gray_old.svg");\n}\n\n.icon-edit {\n  --cr-icon-image: url("/newtab/chromium/images/icon_edit_old.svg");\n}\n\n.icon-file {\n  --cr-icon-image: url("/newtab/chromium/images/icon_filetype_generic_old.svg");\n}\n\n.icon-folder-open {\n  --cr-icon-image: url("/newtab/chromium/images/icon_folder_open_old.svg");\n}\n\n.icon-picture-delete {\n  --cr-icon-image: url("/newtab/chromium/images/icon_picture_delete_old.svg");\n}\n\n.icon-expand-less {\n  --cr-icon-image: url("/newtab/chromium/images/icon_expand_less_old.svg");\n}\n\n.icon-expand-more {\n  --cr-icon-image: url("/newtab/chromium/images/icon_expand_more_old.svg");\n}\n\n.icon-external {\n  --cr-icon-image: url("/newtab/chromium/images/open_in_new_old.svg");\n}\n\n.icon-more-vert {\n  --cr-icon-image: url("/newtab/chromium/images/icon_more_vert_old.svg");\n}\n\n.icon-refresh {\n  --cr-icon-image: url("/newtab/chromium/images/icon_refresh_old.svg");\n}\n\n.icon-search {\n  --cr-icon-image: url("/newtab/chromium/images/icon_search.svg");\n}\n\n.icon-settings {\n  --cr-icon-image: url("/newtab/chromium/images/icon_settings_old.svg");\n}\n\n.icon-visibility {\n  --cr-icon-image: url("/newtab/chromium/images/icon_visibility_old.svg");\n}\n\n.icon-visibility-off {\n  --cr-icon-image: url("/newtab/chromium/images/icon_visibility_off_old.svg");\n}\n\n.subpage-arrow {\n  --cr-icon-image: url("/newtab/chromium/images/arrow_right_old.svg");\n}\n\n.cr-icon {\n  -webkit-mask-image: var(--cr-icon-image);\n  -webkit-mask-position: center;\n  -webkit-mask-repeat: no-repeat;\n  -webkit-mask-size: var(--cr-icon-size);\n  background-color: var(--cr-icon-color, var(--google-grey-700));\n  flex-shrink: 0;\n  height: var(--cr-icon-ripple-size);\n  margin-inline-end: var(--cr-icon-ripple-margin);\n  margin-inline-start: var(--cr-icon-button-margin-start);\n  user-select: none;\n  width: var(--cr-icon-ripple-size);\n}\n\n:host-context([dir=rtl]) .cr-icon {\n  transform: scaleX(-1);  /* Invert X: flip on the Y axis (aka mirror). */\n}\n\n.cr-icon.no-overlap {\n  margin-inline-end: 0;\n  margin-inline-start: 0;\n}\n\n@media (prefers-color-scheme: dark) {\n  .cr-icon {\n    background-color: var(--cr-icon-color, var(--google-grey-500));\n  }\n}\n\n:host-context([webui-rounded-icons]) {\n  .icon-arrow-back {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_back.svg");\n  }\n\n  .icon-arrow-drop-down-cr23 {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_drop_down_cr23.svg");\n  }\n\n  .icon-arrow-drop-up-cr23 {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_drop_up_cr23.svg");\n  }\n\n  .icon-arrow-dropdown {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_dropdown.svg");\n  }\n\n  .icon-arrow-forward {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_forward.svg");\n  }\n\n  .icon-arrow-upward {\n    --cr-icon-image: url("/newtab/chromium/images/icon_arrow_upward.svg");\n  }\n\n  .icon-cancel {\n    --cr-icon-image: url("/newtab/chromium/images/icon_cancel.svg");\n  }\n\n  .icon-clear {\n    --cr-icon-image: url("/newtab/chromium/images/icon_clear.svg");\n  }\n\n  .icon-copy-content {\n    --cr-icon-image: url("/newtab/chromium/images/icon_copy_content.svg");\n  }\n\n  .icon-delete-gray {\n    --cr-icon-image: url("/newtab/chromium/images/icon_delete_gray.svg");\n  }\n\n  .icon-edit {\n    --cr-icon-image: url("/newtab/chromium/images/icon_edit.svg");\n  }\n\n  .icon-expand-less {\n    --cr-icon-image: url("/newtab/chromium/images/icon_expand_less.svg");\n  }\n\n  .icon-expand-more {\n    --cr-icon-image: url("/newtab/chromium/images/icon_expand_more.svg");\n  }\n\n  .icon-external {\n    --cr-icon-image: url("/newtab/chromium/images/open_in_new.svg");\n  }\n\n  .icon-file {\n    --cr-icon-image: url("/newtab/chromium/images/icon_filetype_generic.svg");\n  }\n\n  .icon-folder-open {\n    --cr-icon-image: url("/newtab/chromium/images/icon_folder_open.svg");\n  }\n\n  .icon-more-vert {\n    --cr-icon-image: url("/newtab/chromium/images/icon_more_vert.svg");\n  }\n\n  .icon-picture-delete {\n    --cr-icon-image: url("/newtab/chromium/images/icon_picture_delete.svg");\n  }\n\n  .icon-refresh {\n    --cr-icon-image: url("/newtab/chromium/images/icon_refresh.svg");\n  }\n\n  .icon-settings {\n    --cr-icon-image: url("/newtab/chromium/images/icon_settings.svg");\n  }\n\n  .icon-visibility {\n    --cr-icon-image: url("/newtab/chromium/images/icon_visibility.svg");\n  }\n\n  .icon-visibility-off {\n    --cr-icon-image: url("/newtab/chromium/images/icon_visibility_off.svg");\n  }\n\n  .subpage-arrow {\n    --cr-icon-image: url("/newtab/chromium/images/arrow_right.svg");\n  }\n}\n'])];
}
function getCss8() {
  return [i(["/* Copyright 2025 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #import=./cr_shared_vars.css.js\n * #css_wrapper_metadata_end */\n\n/* Common CSS classes used to make an element scrollable and add anchored\n * elements at the top and bottom of the scrollable element.  The anchored top\n * and bottom elements show either a border or a shadow when the cr-scrollable\n * element is scrollable in their respective directions. */\n\n.cr-scrollable {\n  anchor-name: --cr-scrollable;\n  anchor-scope: --cr-scrollable;\n  container-type: scroll-state;\n  overflow: auto;\n}\n\n.cr-scrollable-top,\n.cr-scrollable-top-shadow,\n.cr-scrollable-bottom,\n.cr-scrollable-bottom-shadow {\n  display: none;\n  position: fixed;\n  position-anchor: --cr-scrollable;\n  left: anchor(left);\n  width: anchor-size(width);\n  pointer-events: none;\n\n  &:where(.force-on) {\n    display: block;\n  }\n}\n\n.cr-scrollable-top {\n  top: anchor(top);\n  border-top: 1px solid var(--cr-scrollable-border-color);\n\n  @container scroll-state(scrollable: top) {\n    display: block;\n  }\n}\n\n.cr-scrollable-bottom {\n  bottom: anchor(bottom);\n  border-bottom: 1px solid var(--cr-scrollable-border-color);\n\n  @container scroll-state(scrollable: bottom) {\n    display: block;\n  }\n}\n\n.cr-scrollable-top-shadow,\n.cr-scrollable-bottom-shadow {\n  box-shadow: inset 0 5px 6px -3px rgba(0, 0, 0, .4);\n  display: block;\n  height: 8px;\n  opacity: 0;\n  top: anchor(top);\n  transition: opacity 500ms;\n  z-index: 1;\n\n  &:where(.force-on) {\n    opacity: 1;\n  }\n}\n\n.cr-scrollable-top-shadow {\n  @container scroll-state(scrollable: top) {\n    opacity: 1;\n  }\n}\n\n.cr-scrollable-bottom-shadow {\n  top: auto;\n  bottom: anchor(bottom);\n  transform: scale(-1);\n\n  @container scroll-state(scrollable: bottom) {\n    opacity: 1;\n  }\n}\n"])];
}
function getCss9() {
  return [getCss3(), getCss7(), getCss8(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_shared_vars.css.js\n * #import=../cr_hidden_style_lit.css.js\n * #import=../cr_icons_lit.css.js\n * #import=../cr_scrollable_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit cr-icons-lit cr-scrollable-lit\n * #css_wrapper_metadata_end */\n\ndialog {\n  background-color: var(--cr-dialog-background-color, white);\n  border: 0;\n  border-radius: var(--cr-dialog-border-radius, 8px);\n  bottom: 50%;\n  box-shadow: 0 0 16px rgba(0, 0, 0, 0.12),\n              0 16px 16px rgba(0, 0, 0, 0.24);\n  color: inherit;\n  line-height: 20px;\n  max-height: initial;\n  max-width: initial;\n  overflow-y: hidden;\n  padding: 0;\n  position: absolute;\n  top: 50%;\n  width: var(--cr-dialog-width, 512px);\n}\n\n@media (prefers-color-scheme: dark) {\n  dialog {\n    background-color: var(--cr-dialog-background-color,\n        var(--google-grey-900));\n    /* Note: the colors in linear-gradient() are intentionally the same to\n     * add a 4% white layer on top of the fully opaque background-color. */\n    background-image: linear-gradient(rgba(255, 255, 255, .04),\n                                      rgba(255, 255, 255, .04));\n  }\n}\n\n:host-context([webui-refresh-2026]) {\n  dialog {\n    background-color: var(--cr-dialog-background-color,\n        var(--color-webui-dialog-background));\n  }\n}\n\n@media (forced-colors: active) {\n  dialog {\n    /* Use border instead of box-shadow (which does not work) in Windows\n       HCM. */\n    border: var(--cr-border-hcm);\n  }\n}\n\ndialog[open] #content-wrapper {\n  /* Keep max-height within viewport, and flex content accordingly. */\n  display: flex;\n  flex-direction: column;\n  max-height: 100vh;\n  overflow: auto;\n}\n\n/* When needing to flex, force .body-container alone to shrink. */\n.top-container,\n:host ::slotted([slot=button-container]),\n:host ::slotted([slot=footer]) {\n  flex-shrink: 0;\n}\n\ndialog::backdrop {\n  background-color: rgba(0, 0, 0, 0.6);\n  bottom: 0;\n  left: 0;\n  position: fixed;\n  right: 0;\n  top: 0;\n}\n\n:host ::slotted([slot=body]) {\n  color: var(--cr-secondary-text-color);\n  padding: 0 var(--cr-dialog-body-padding-horizontal, 20px);\n}\n\n:host ::slotted([slot=title]) {\n  color: var(--cr-primary-text-color);\n  flex: 1;\n  font-family: var(--cr-dialog-font-family, inherit);\n  font-size: var(--cr-dialog-title-font-size, calc(15 / 13 * 100%));\n  line-height: 1;\n  padding-bottom: var(--cr-dialog-title-slot-padding-bottom, 16px);\n  padding-inline-end:  var(--cr-dialog-title-slot-padding-end, 20px);\n  padding-inline-start: var(--cr-dialog-title-slot-padding-start, 20px);\n  padding-top: var(--cr-dialog-title-slot-padding-top, 20px);\n}\n\n/* Note that if the padding is non-uniform and the button-container\n * border is visible, then the buttons will appear off-center. */\n:host ::slotted([slot=button-container]) {\n  display: flex;\n  justify-content: flex-end;\n  padding-bottom: var(--cr-dialog-button-container-padding-bottom, 16px);\n  padding-inline-end: var(--cr-dialog-button-container-padding-horizontal, 16px);\n  padding-inline-start: var(--cr-dialog-button-container-padding-horizontal, 16px);\n  padding-top: var(--cr-dialog-button-container-padding-top, 16px);\n}\n\n:host ::slotted([slot=footer]) {\n  border-bottom-left-radius: inherit;\n  border-bottom-right-radius: inherit;\n  border-top: 1px solid #dbdbdb;\n  margin: 0;\n  padding: 16px 20px;\n}\n\n:host([hide-backdrop]) dialog::backdrop {\n  opacity: 0;\n}\n\n@media (prefers-color-scheme: dark) {\n  :host ::slotted([slot=footer]) {\n    border-top-color: var(--cr-separator-color);\n  }\n}\n\n.body-container {\n  box-sizing: border-box;\n  display: flex;\n  flex-direction: column;\n  min-height: 1.375rem; /* Minimum reasonably usable height. */\n  overflow: auto;\n}\n\n.top-container {\n  align-items: flex-start;\n  display: flex;\n  min-height: var(--cr-dialog-top-container-min-height, 31px);\n}\n\n.title-container {\n  display: flex;\n  flex: 1;\n  font-size: inherit;\n  font-weight: inherit;\n  margin: 0;\n  outline: none;\n}\n\n#close {\n  align-self: flex-start;\n  margin-inline-end: 4px;\n  margin-top: 4px;\n}\n\n/* If --cr-dialog-body-border-top is defined, force show the scrollable top\n * border and override its styling. */\n@container style(--cr-dialog-body-border-top) {\n  .cr-scrollable-top {\n    display: block;\n    border-top: var(--cr-dialog-body-border-top);\n  }\n}\n"])];
}
function getHtml4() {
  return b2`
<dialog id="dialog" @close="${this.onNativeDialogClose_}"
    @cancel="${this.onNativeDialogCancel_}" part="dialog"
    aria-labelledby="title"
    aria-description="${this.ariaDescriptionText || A}"
    closedby="${this.noCancel ? "none" : A}">
  <!-- This wrapper is necessary, such that the "pulse" animation is not
    erroneously played when the user clicks on the outer-most scrollbar. -->
  <div id="content-wrapper" part="wrapper">
    <div class="top-container">
      <h2 id="title" class="title-container" tabindex="-1">
        <slot name="title"></slot>
      </h2>
      ${this.showCloseButton ? b2`
        <cr-icon-button id="close" class="icon-clear"
            aria-label="${this.closeText || A}"
            title="${this.closeText || A}" @click="${this.onCloseClick_}"
            @keypress="${this.onCloseKeypress_}">
        </cr-icon-button>
      ` : ""}
    </div>
    <slot name="header"></slot>
    <div class="body-container cr-scrollable" id="container"
        part="body-container">
      <div class="cr-scrollable-top"></div>
      <slot name="body"></slot>
      <div class="cr-scrollable-bottom"></div>
    </div>
    <slot name="button-container"></slot>
    <slot name="footer"></slot>
  </div>
</dialog>`;
}
var CrDialogElement = class extends CrLitElement {
  static get is() {
    return "cr-dialog";
  }
  static get styles() {
    return getCss9();
  }
  render() {
    return getHtml4.bind(this)();
  }
  static get properties() {
    return {
      open: {
        type: Boolean,
        reflect: true
      },
      /**
       * Alt-text for the dialog close button.
       */
      closeText: { type: String },
      /**
       * True if the dialog should remain open on 'popstate' events. This is
       * used for navigable dialogs that have their separate navigation handling
       * code.
       */
      ignorePopstate: { type: Boolean },
      /**
       * True if the dialog should ignore 'Enter' keypresses.
       */
      ignoreEnterKey: { type: Boolean },
      /**
       * True if the dialog should consume 'keydown' events. If ignoreEnterKey
       * is true, 'Enter' key won't be consumed.
       */
      consumeKeydownEvent: { type: Boolean },
      /**
       * True if the dialog should not be able to be cancelled, which will
       * prevent 'Escape' key presses from closing the dialog.
       */
      noCancel: { type: Boolean },
      // True if dialog should show the 'X' close button.
      showCloseButton: { type: Boolean },
      showOnAttach: { type: Boolean },
      /**
       * Text for the aria description.
       */
      ariaDescriptionText: { type: String }
    };
  }
  #closeText;
  get closeText() {
    return this.#closeText;
  }
  set closeText(_2) {
    this.#closeText = _2;
  }
  #consumeKeydownEvent = false;
  get consumeKeydownEvent() {
    return this.#consumeKeydownEvent;
  }
  set consumeKeydownEvent(_2) {
    this.#consumeKeydownEvent = _2;
  }
  #ignoreEnterKey = false;
  get ignoreEnterKey() {
    return this.#ignoreEnterKey;
  }
  set ignoreEnterKey(_2) {
    this.#ignoreEnterKey = _2;
  }
  #ignorePopstate = false;
  get ignorePopstate() {
    return this.#ignorePopstate;
  }
  set ignorePopstate(_2) {
    this.#ignorePopstate = _2;
  }
  #noCancel = false;
  get noCancel() {
    return this.#noCancel;
  }
  set noCancel(_2) {
    this.#noCancel = _2;
  }
  #open = false;
  get open() {
    return this.#open;
  }
  set open(_2) {
    this.#open = _2;
  }
  #showCloseButton = false;
  get showCloseButton() {
    return this.#showCloseButton;
  }
  set showCloseButton(_2) {
    this.#showCloseButton = _2;
  }
  #showOnAttach = false;
  get showOnAttach() {
    return this.#showOnAttach;
  }
  set showOnAttach(_2) {
    this.#showOnAttach = _2;
  }
  #ariaDescriptionText;
  get ariaDescriptionText() {
    return this.#ariaDescriptionText;
  }
  set ariaDescriptionText(_2) {
    this.#ariaDescriptionText = _2;
  }
  mutationObserver_ = null;
  boundKeydown_ = null;
  tracker_ = new EventTracker();
  connectedCallback() {
    super.connectedCallback();
    const mutationObserverCallback = () => {
      if (this.$.dialog.open) {
        this.addKeydownListener_();
      } else {
        this.removeKeydownListener_();
      }
    };
    this.mutationObserver_ = new MutationObserver(mutationObserverCallback);
    this.mutationObserver_.observe(this.$.dialog, {
      attributes: true,
      attributeFilter: ["open"]
    });
    mutationObserverCallback();
    if (this.showOnAttach) {
      this.showModal();
    }
    this.tracker_.add(window, "popstate", () => {
      if (!this.ignorePopstate && this.$.dialog.open) {
        this.cancel();
      }
    });
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    this.removeKeydownListener_();
    if (this.mutationObserver_) {
      this.mutationObserver_.disconnect();
      this.mutationObserver_ = null;
    }
    this.tracker_.removeAll();
  }
  firstUpdated() {
    if (!this.ignoreEnterKey) {
      this.addEventListener("keypress", this.onKeypress_.bind(this));
    }
    this.addEventListener("pointerdown", (e5) => this.onPointerdown_(e5));
  }
  addKeydownListener_() {
    if (!this.consumeKeydownEvent) {
      return;
    }
    this.boundKeydown_ = this.boundKeydown_ || this.onKeydown_.bind(this);
    this.addEventListener("keydown", this.boundKeydown_);
    document.body.addEventListener("keydown", this.boundKeydown_);
  }
  removeKeydownListener_() {
    if (!this.boundKeydown_) {
      return;
    }
    this.removeEventListener("keydown", this.boundKeydown_);
    document.body.removeEventListener("keydown", this.boundKeydown_);
    this.boundKeydown_ = null;
  }
  async showModal() {
    if (this.showOnAttach) {
      const element = this.querySelector("[autofocus]");
      if (element && element instanceof CrLitElement && !element.shadowRoot) {
        element.ensureInitialRender();
      }
    }
    this.$.dialog.showModal();
    assert(this.$.dialog.open);
    this.open = true;
    await this.updateComplete;
    this.fire("cr-dialog-open");
  }
  onCloseClick_() {
    this.cancel();
  }
  cancel() {
    this.fire("cancel");
    this.$.dialog.close();
    assert(!this.$.dialog.open);
    this.open = false;
  }
  close() {
    this.$.dialog.close("success");
    assert(!this.$.dialog.open);
    this.open = false;
  }
  /**
   * Set the title of the dialog for a11y reader.
   * @param title Title of the dialog.
   */
  setTitleAriaLabel(title) {
    this.$.dialog.removeAttribute("aria-labelledby");
    this.$.dialog.setAttribute("aria-label", title);
  }
  onCloseKeypress_(e5) {
    e5.stopPropagation();
  }
  onNativeDialogClose_(e5) {
    if (e5.target !== this.getNative()) {
      return;
    }
    this.fire("close");
  }
  async onNativeDialogCancel_(e5) {
    if (e5.target !== this.getNative()) {
      return;
    }
    this.open = false;
    await this.updateComplete;
    this.fire("cancel");
  }
  /**
   * Expose the inner native <dialog> for some rare cases where it needs to be
   * directly accessed (for example to programmatically setheight/width, which
   * would not work on the wrapper).
   */
  getNative() {
    return this.$.dialog;
  }
  onKeypress_(e5) {
    if (e5.key !== "Enter") {
      return;
    }
    const accept = e5.target === this || e5.composedPath().some(
      (el) => el.tagName === "CR-INPUT" && el.type !== "search"
    );
    if (!accept) {
      return;
    }
    const actionButton = this.querySelector(
      ".action-button:not([disabled]):not([hidden])"
    );
    if (actionButton) {
      actionButton.click();
      e5.preventDefault();
    }
  }
  onKeydown_(e5) {
    assert(this.consumeKeydownEvent);
    if (!this.getNative().open) {
      return;
    }
    if (this.ignoreEnterKey && e5.key === "Enter") {
      return;
    }
    e5.stopPropagation();
  }
  onPointerdown_(e5) {
    if (e5.button !== 0 || e5.composedPath()[0].tagName !== "DIALOG") {
      return;
    }
    this.$.dialog.animate(
      [
        { transform: "scale(1)", offset: 0 },
        { transform: "scale(1.02)", offset: 0.4 },
        { transform: "scale(1.02)", offset: 0.6 },
        { transform: "scale(1)", offset: 1 }
      ],
      {
        duration: 180,
        easing: "ease-in-out",
        iterations: 1
      }
    );
    e5.preventDefault();
  }
  focus() {
    const titleContainer = this.shadowRoot.querySelector(".title-container");
    assert(titleContainer);
    titleContainer.focus();
  }
};
customElements.define(CrDialogElement.is, CrDialogElement);
function getCss10() {
  return [getCss3(), getCss7(), i(["/* Copyright 2022 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=./cr_shared_vars.css.js\n * #import=./cr_hidden_style_lit.css.js\n * #import=./cr_icons_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit cr-icons-lit\n * #css_wrapper_metadata_end */\n\n[actionable] {\n  cursor: pointer;\n}\n\n/* Horizontal rule line. */\n.hr {\n  border-top: var(--cr-separator-line);\n}\n\niron-list.cr-separators > *:not([first]) {\n  border-top: var(--cr-separator-line);\n}\n\n[scrollable] {\n  border-color: transparent;\n  border-style: solid;\n  border-width: 1px 0;\n  overflow-y: auto;\n}\n\n[scrollable].is-scrolled {\n  border-top-color: var(--cr-scrollable-border-color);\n}\n\n[scrollable].can-scroll:not(.scrolled-to-bottom) {\n  border-bottom-color: var(--cr-scrollable-border-color);\n}\n\n[scrollable] iron-list > :not(.no-outline):focus-visible,\n[selectable]:focus-visible,\n[selectable] > :focus-visible {\n  outline: solid 2px var(--cr-focus-outline-color);\n  /* Selectable lists usually take the full width of their containers and\n   * have their overflows clipped, so the outlines need to inset. */\n  outline-offset: -2px;\n}\n\n.scroll-container {\n  display: flex;\n  flex-direction: column;\n  min-height: 1px;\n}\n\n[selectable] > * {\n  cursor: pointer;\n}\n\n.cr-centered-card-container {\n  box-sizing: border-box;\n  display: block;\n  height: inherit;\n  margin: 0 auto;\n  max-width: var(--cr-centered-card-max-width);\n  min-width: 550px;\n  position: relative;\n  width: calc(100% * var(--cr-centered-card-width-percentage));\n}\n\n.cr-row {\n  align-items: center;\n  border-top: var(--cr-separator-line);\n  display: flex;\n  min-height: var(--cr-section-min-height);\n  padding: 0 var(--cr-section-padding);\n}\n\n.cr-row.first,\n.cr-row.continuation {\n  border-top: none;\n}\n\n.cr-row-gap {\n  padding-inline-start: 16px;\n}\n\n.cr-button-gap {\n  margin-inline-start: 8px;\n}\n\npaper-tooltip::part(tooltip),\ncr-tooltip::part(tooltip) {\n  border-radius: var(--paper-tooltip-border-radius, 2px);\n  font-size: 92.31%;  /* Effectively 12px if the host default is 13px. */\n  font-weight: 500;\n  max-width: 330px;\n  min-width: var(--paper-tooltip-min-width, 200px);\n  padding: var(--paper-tooltip-padding, 10px 8px);\n}\n\n/* Typography */\n\n.cr-padded-text {\n  padding-block-end: var(--cr-section-vertical-padding);\n  padding-block-start: var(--cr-section-vertical-padding);\n}\n\n.cr-title-text {\n  color: var(--cr-title-text-color);\n  font-size: 107.6923%; /* Go to 14px from 13px. */\n  font-weight: 500;\n}\n\n.cr-secondary-text {\n  color: var(--cr-secondary-text-color);\n  font-weight: 400;\n}\n\n.cr-form-field-label {\n  color: var(--cr-form-field-label-color);\n  display: block;\n  font-size: var(--cr-form-field-label-font-size);\n  font-weight: 500;\n  letter-spacing: .4px;\n  line-height: var(--cr-form-field-label-line-height);\n  margin-bottom: 8px;\n}\n\n.cr-vertical-tab {\n  align-items: center;\n  display: flex;\n}\n\n.cr-vertical-tab::before {\n  border-radius: 0 3px 3px 0;\n  content: '';\n  display: block;\n  flex-shrink: 0;\n  height: var(--cr-vertical-tab-height, 100%);\n  width: 4px;\n}\n\n.cr-vertical-tab.selected::before {\n  background: var(--cr-vertical-tab-selected-color, var(--cr-checked-color));\n}\n\n:host-context([dir=rtl]) .cr-vertical-tab::before {\n  /* Border-radius based on block/inline is not yet supported. */\n  transform: scaleX(-1);\n}\n\n.iph-anchor-highlight {\n  background-color: var(--cr-iph-anchor-highlight-color);\n}\n"])];
}
function getCss11() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_shared_vars.css.js\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-input-background-color: var(--color-textfield-filled-background,\n      var(--cr-fallback-color-surface-variant));\n  --cr-input-border-bottom: 1px solid\n      var(--color-textfield-filled-underline,\n          var(--cr-fallback-color-outline));\n  --cr-input-border-radius: 8px 8px 0 0;\n  --cr-input-color: var(--cr-primary-text-color);\n  --cr-input-error-color: var(--color-textfield-filled-error,\n      var(--cr-fallback-color-error));\n  --cr-input-focus-color: var(--color-textfield-filled-underline-focused,\n      var(--cr-fallback-color-primary));\n  --cr-input-hover-background-color: var(--cr-hover-background-color);\n  --cr-input-label-color: var(--color-textfield-foreground-label,\n      var(--cr-fallback-color-on-surface-subtle));\n  --cr-input-padding-bottom: 10px;\n  --cr-input-padding-end: 10px;\n  --cr-input-padding-start: 10px;\n  --cr-input-padding-top: 10px;\n  --cr-input-placeholder-color:\n      var(--color-textfield-foreground-placeholder,\n          var(--cr-fallback-on-surface-subtle));\n  display: block;\n  isolation: isolate;\n  /* Avoid showing outline when focus() programmatically called multiple\n     times in a row. */\n  outline: none;\n}\n\n:host([readonly]) {\n  --cr-input-border-radius: 8px 8px;\n}\n\n#label {\n  color: var(--cr-input-label-color);\n  font-size: 11px;\n  line-height: 16px;\n}\n\n:host([focused_]:not([readonly]):not([invalid])) #label {\n  color: var(--cr-input-focus-label-color, var(--cr-input-label-color));\n}\n\n/* Input styling below. */\n#input-container {\n  border-radius: var(--cr-input-border-radius, 4px);\n  overflow: hidden;\n  position: relative;\n  width: var(--cr-input-width, 100%);\n}\n\n:host([focused_]) #input-container {\n  outline: var(--cr-input-focus-outline, none);\n}\n\n#inner-input-container {\n  background-color: var(--cr-input-background-color);\n  box-sizing: border-box;\n  padding: 0;\n}\n\n#inner-input-content ::slotted(*) {\n  --cr-icon-button-fill-color: var(--color-textfield-foreground-icon,\n      var(--cr-fallback-color-on-surface-subtle));\n  --cr-icon-button-icon-size: 16px;\n  --cr-icon-button-size: 24px;\n  --cr-icon-button-margin-start: 0;\n  --cr-icon-color: var(--color-textfield-foreground-icon,\n      var(--cr-fallback-color-on-surface-subtle));\n}\n\n#inner-input-content ::slotted([slot='inline-prefix']) {\n  --cr-icon-button-margin-start: -8px;\n}\n\n#inner-input-content ::slotted([slot='inline-suffix']) {\n  --cr-icon-button-margin-end: -4px;\n}\n\n:host([invalid]) #inner-input-content ::slotted(*) {\n  --cr-icon-color: var(--cr-input-error-color);\n  --cr-icon-button-fill-color: var(--cr-input-error-color);\n}\n\n#hover-layer {\n  background-color: var(--cr-input-hover-background-color);\n  display: none;\n  inset: 0;\n  pointer-events: none;\n  position: absolute;\n  z-index: 0;\n}\n\n:host(:not([readonly]):not([disabled]))\n    #input-container:hover #hover-layer {\n  display: block;\n}\n\n#input {\n  -webkit-appearance: none;\n  /* Transparent, #inner-input-container will apply background. */\n  background-color: transparent;\n  border: none;\n  box-sizing: border-box;\n  caret-color: var(--cr-input-focus-color);\n  color: var(--cr-input-color);\n  font-family: inherit;\n  font-size: var(--cr-input-font-size, 12px);\n  font-weight: inherit;\n  line-height: 16px;\n  min-height: var(--cr-input-min-height, auto);\n  outline: none;\n  padding: 0;\n  text-align: inherit;\n  text-overflow: ellipsis;\n  width: 100%;\n}\n\n#inner-input-content {\n  padding-bottom: var(--cr-input-padding-bottom);\n  padding-inline-end: var(--cr-input-padding-end);\n  padding-inline-start: var(--cr-input-padding-start);\n  padding-top: var(--cr-input-padding-top);\n}\n\n/* Underline styling below. */\n#underline {\n  border-bottom: 2px solid var(--cr-input-focus-color);\n  border-radius: var(--cr-input-underline-border-radius, 0);\n  bottom: 0;\n  box-sizing: border-box;\n  display: var(--cr-input-underline-display);\n  height: var(--cr-input-underline-height, 0);\n  left: 0;\n  margin: auto;\n  opacity: 0;\n  position: absolute;\n  right: 0;\n  transition: opacity 120ms ease-out, width 0s linear 180ms;\n  width: 0;\n}\n\n:host([invalid]) #underline,\n:host([force-underline]) #underline,\n:host([focused_]) #underline {\n  opacity: 1;\n  transition: opacity 120ms ease-in, width 180ms ease-out;\n  width: 100%;\n}\n\n#underline-base {\n  display: none;\n}\n\n:host([readonly]) #underline {\n  display: none;\n}\n\n:host(:not([readonly])) #underline-base {\n  border-bottom: var(--cr-input-border-bottom);\n  bottom: 0;\n  display: block;\n  left: 0;\n  position: absolute;\n  right: 0;\n}\n\n:host([disabled]) {\n  color: var(--color-textfield-foreground-disabled,\n      var(--cr-fallback-color-disabled-foreground));\n  --cr-input-border-bottom: 1px solid currentColor;\n  --cr-input-placeholder-color: currentColor;\n  --cr-input-color: currentColor;\n  --cr-input-background-color: var(--color-textfield-background-disabled,\n      var(--cr-fallback-color-disabled-background));\n}\n\n:host([disabled]) #inner-input-content ::slotted(*) {\n  --cr-icon-color: currentColor;\n  --cr-icon-button-fill-color: currentColor;\n}\n\n:host(.stroked) {\n  --cr-input-background-color: transparent;\n  --cr-input-border: 1px solid var(--color-side-panel-textfield-border,\n      var(--cr-fallback-color-neutral-outline));\n  --cr-input-border-bottom: none;\n  --cr-input-border-radius: 8px;\n  --cr-input-padding-bottom: 9px;\n  --cr-input-padding-end: 9px;\n  --cr-input-padding-start: 9px;\n  --cr-input-padding-top: 9px;\n  --cr-input-underline-display: none;\n  --cr-input-min-height: 36px;\n  line-height: 16px;\n}\n\n:host(.stroked[focused_]) {\n  --cr-input-border: 2px solid var(--cr-focus-outline-color);\n  --cr-input-padding-bottom: 8px;\n  --cr-input-padding-end: 8px;\n  --cr-input-padding-start: 8px;\n  --cr-input-padding-top: 8px;\n}\n\n:host(.stroked[invalid]) {\n  --cr-input-border: 1px solid var(--cr-input-error-color);\n}\n\n:host(.stroked[focused_][invalid]) {\n  --cr-input-border: 2px solid var(--cr-input-error-color);\n}\n"])];
}
function getCss12() {
  return [getCss3(), getCss10(), getCss11(), i([`/* Copyright 2024 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=../cr_shared_vars.css.js
 * #import=../cr_hidden_style_lit.css.js
 * #import=../cr_shared_style_lit.css.js
 * #import=./cr_input_style_lit.css.js
 * #scheme=relative
 * #include=cr-hidden-style-lit cr-input-style-lit cr-shared-style-lit
 * #css_wrapper_metadata_end */

/*
  A 'suffix' element will be outside the underlined space, while a
  'inline-prefix' and 'inline-suffix' elements will be inside the
  underlined space by default.

  Regarding cr-input's width:
  When there's no element in the 'inline-prefix', 'inline-suffix' or
  'suffix' slot, setting the width of cr-input as follows will work as
  expected:

    cr-input {
      width: 200px;
    }

  However, when there's an element in the 'suffix', 'inline-suffix' and/or
  'inline-prefix' slot, setting the 'width' will dictate the total width
  of the input field *plus* the 'inline-prefix', 'inline-suffix' and
  'suffix' elements. To set the width of the input field +
  'inline-prefix' + 'inline-suffix' when a 'suffix' is present,
  use --cr-input-width.

    cr-input {
      --cr-input-width: 200px;
    }
*/

/* Disabled status should not impact suffix slot. */
:host([disabled]) :-webkit-any(#label, #error, #input-container) {
  opacity: var(--cr-disabled-opacity);
  pointer-events: none;
}

:host([disabled]) :is(#label, #error, #input-container) {
  opacity: 1;
}

/* Margin between <input> and <cr-button> in the 'suffix' slot */
:host ::slotted(cr-button[slot=suffix]) {
  margin-inline-start: var(--cr-button-edge-spacing) !important;
}

:host([invalid]) #label {
  color: var(--cr-input-error-color);
}

#input {
  border-bottom: none;
  letter-spacing: var(--cr-input-letter-spacing);
}

#input-container {
  border: var(--cr-input-border, none);
}

#input::placeholder {
  color: var(--cr-input-placeholder-color, var(--cr-secondary-text-color));
  letter-spacing: var(--cr-input-placeholder-letter-spacing);
}

:host([invalid]) #input {
  caret-color: var(--cr-input-error-color);
}

:host([readonly]) #input {
  opacity: var(--cr-input-readonly-opacity, 0.6);
}

:host([invalid]) #underline {
  border-color: var(--cr-input-error-color);
}

/* Error styling below. */
#error {
  /* Defaults to "display: block" and "visibility:hidden" to allocate
     space for error message, such that the page does not shift when
     error appears. For cr-inputs that can't be invalid, but are in a
     form with cr-inputs that can be invalid, this space is also desired
     in order to have consistent spacing.

     If spacing is not needed, apply "--cr-input-error-display: none".

     When grouping cr-inputs horizontally, it might be helpful to set
     --cr-input-error-white-space to "nowrap" and set a fixed width for
     each cr-input so that a long error label does not shift the inputs
     forward. */
  color: var(--cr-input-error-color);
  display: var(--cr-input-error-display, block);
  font-size: 11px;
  min-height: var(--cr-form-field-label-height);
  line-height: 16px;
  margin: 4px 10px;
  visibility: hidden;
  white-space: var(--cr-input-error-white-space);
  height: auto;
  overflow: hidden;
  text-overflow: ellipsis;
}

:host([invalid]) #error {
  visibility: visible;
}

#row-container,
#inner-input-content {
  align-items: center;
  display: flex;
  /* This will spread the input field and the suffix apart only if the
     host element width is intentionally set to something large. */
  justify-content: space-between;
  position: relative;
}

#inner-input-content {
  gap: 4px;
  height: 16px;
  /* Ensures content sits above the hover layer */
  z-index: 1;
}

#input[type='search']::-webkit-search-cancel-button {
  display: none;
}

:host-context([dir=rtl]) #input[type=url] {
  text-align: right;  /* csschecker-disable-line left-right */
}

#input[type=url] {
  direction: ltr;
}
`])];
}
function getHtml5() {
  return b2`
<div id="label" class="cr-form-field-label" ?hidden="${!this.label}"
    aria-hidden="${this.getLabelAriaHidden_() || A}">
  ${this.label}
</div>
<div id="row-container" part="row-container">
  <div id="input-container">
    <div id="inner-input-container">
      <div id="hover-layer"></div>
      <div id="inner-input-content">
        <slot name="inline-prefix"></slot>
        <input id="input" ?disabled="${this.disabled}"
            ?autofocus="${this.autofocus}" .value="${this.internalValue_}"
            tabindex="${this.inputTabindex}" .type="${this.type}"
            ?readonly="${this.readonly}" maxlength="${this.maxlength}"
            pattern="${this.pattern || A}" ?required="${this.required}"
            minlength="${this.minlength}" inputmode="${this.inputmode}"
            aria-description="${this.ariaDescription || A}"
            aria-errormessage="${this.getAriaErrorMessage_() || A}"
            aria-labelledby="${this.getAriaLabelledBy_() || A}"
            aria-label="${this.getAriaLabel_()}"
            aria-invalid="${this.getAriaInvalid_()}"
            .max="${this.max || A}" .min="${this.min || A}"
            @focus="${this.onInputFocus_}" @blur="${this.onInputBlur_}"
            @change="${this.onInputChange_}" @input="${this.onInput_}"
            part="input" autocomplete="off">
        <slot name="inline-suffix"></slot>
      </div>
    </div>
    <div id="underline-base"></div>
    <div id="underline"></div>
  </div>
  <slot name="suffix"></slot>
</div>
<div id="error" role="${this.getErrorRole_() || A}"
    aria-live="assertive">${this.getErrorMessage_()}</div>`;
}
var SUPPORTED_INPUT_TYPES = /* @__PURE__ */ new Set([
  "number",
  "password",
  "search",
  "text",
  "url"
]);
var CrInputElement = class extends CrLitElement {
  static get is() {
    return "cr-input";
  }
  static get styles() {
    return getCss12();
  }
  render() {
    return getHtml5.bind(this)();
  }
  static get properties() {
    return {
      ariaDescription: { type: String },
      ariaLabel: { type: String },
      autofocus: {
        type: Boolean,
        reflect: true
      },
      autoValidate: { type: Boolean },
      disabled: {
        type: Boolean,
        reflect: true
      },
      errorMessage: { type: String },
      /**
       * This is strictly used internally for styling, do not attempt to use
       * this to set focus.
       */
      focused_: {
        type: Boolean,
        reflect: true
      },
      invalid: {
        type: Boolean,
        notify: true,
        reflect: true
      },
      max: {
        type: Number,
        reflect: true
      },
      min: {
        type: Number,
        reflect: true
      },
      maxlength: {
        type: Number,
        reflect: true
      },
      minlength: {
        type: Number,
        reflect: true
      },
      pattern: {
        type: String,
        reflect: true
      },
      inputmode: { type: String },
      label: { type: String },
      placeholder: { type: String },
      readonly: {
        type: Boolean,
        reflect: true
      },
      required: {
        type: Boolean,
        reflect: true
      },
      inputTabindex: { type: Number },
      type: { type: String },
      value: {
        type: String,
        notify: true
      },
      internalValue_: {
        type: String,
        state: true
      }
    };
  }
  #ariaDescription = null;
  get ariaDescription() {
    return this.#ariaDescription;
  }
  set ariaDescription(_2) {
    this.#ariaDescription = _2;
  }
  #ariaLabel = "";
  get ariaLabel() {
    return this.#ariaLabel;
  }
  set ariaLabel(_2) {
    this.#ariaLabel = _2;
  }
  #autofocus = false;
  get autofocus() {
    return this.#autofocus;
  }
  set autofocus(_2) {
    this.#autofocus = _2;
  }
  #autoValidate = false;
  get autoValidate() {
    return this.#autoValidate;
  }
  set autoValidate(_2) {
    this.#autoValidate = _2;
  }
  #disabled = false;
  get disabled() {
    return this.#disabled;
  }
  set disabled(_2) {
    this.#disabled = _2;
  }
  #errorMessage = "";
  get errorMessage() {
    return this.#errorMessage;
  }
  set errorMessage(_2) {
    this.#errorMessage = _2;
  }
  #inputmode;
  get inputmode() {
    return this.#inputmode;
  }
  set inputmode(_2) {
    this.#inputmode = _2;
  }
  #inputTabindex = 0;
  get inputTabindex() {
    return this.#inputTabindex;
  }
  set inputTabindex(_2) {
    this.#inputTabindex = _2;
  }
  #invalid = false;
  get invalid() {
    return this.#invalid;
  }
  set invalid(_2) {
    this.#invalid = _2;
  }
  #label = "";
  get label() {
    return this.#label;
  }
  set label(_2) {
    this.#label = _2;
  }
  #max;
  get max() {
    return this.#max;
  }
  set max(_2) {
    this.#max = _2;
  }
  #min;
  get min() {
    return this.#min;
  }
  set min(_2) {
    this.#min = _2;
  }
  #maxlength;
  get maxlength() {
    return this.#maxlength;
  }
  set maxlength(_2) {
    this.#maxlength = _2;
  }
  #minlength;
  get minlength() {
    return this.#minlength;
  }
  set minlength(_2) {
    this.#minlength = _2;
  }
  #pattern;
  get pattern() {
    return this.#pattern;
  }
  set pattern(_2) {
    this.#pattern = _2;
  }
  #placeholder = null;
  get placeholder() {
    return this.#placeholder;
  }
  set placeholder(_2) {
    this.#placeholder = _2;
  }
  #readonly = false;
  get readonly() {
    return this.#readonly;
  }
  set readonly(_2) {
    this.#readonly = _2;
  }
  #required = false;
  get required() {
    return this.#required;
  }
  set required(_2) {
    this.#required = _2;
  }
  #type = "text";
  get type() {
    return this.#type;
  }
  set type(_2) {
    this.#type = _2;
  }
  #value = "";
  get value() {
    return this.#value;
  }
  set value(_2) {
    this.#value = _2;
  }
  #internalValue_ = "";
  get internalValue_() {
    return this.#internalValue_;
  }
  set internalValue_(_2) {
    this.#internalValue_ = _2;
  }
  #focused_ = false;
  get focused_() {
    return this.#focused_;
  }
  set focused_(_2) {
    this.#focused_ = _2;
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("value")) {
      this.internalValue_ = this.value === void 0 || this.value === null ? "" : this.value;
    }
    if (changedProperties.has("inputTabindex")) {
      assert(this.inputTabindex === 0 || this.inputTabindex === -1);
    }
    if (changedProperties.has("type")) {
      assert(SUPPORTED_INPUT_TYPES.has(this.type));
    }
  }
  firstUpdated() {
    assert(!this.hasAttribute("tabindex"));
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("value")) {
      const previous = changedProperties.get("value");
      if ((!!this.value || !!previous) && this.autoValidate) {
        this.invalid = !this.inputElement.checkValidity();
      }
    }
    if (changedProperties.has("placeholder")) {
      if (this.placeholder === null || this.placeholder === void 0) {
        this.inputElement.removeAttribute("placeholder");
      } else {
        this.inputElement.setAttribute("placeholder", this.placeholder);
      }
    }
  }
  get inputElement() {
    return this.$.input;
  }
  focus() {
    this.focusInput();
  }
  /**
   * Focuses the input element.
   * TODO(crbug.com/40593040): Replace this with focus() after resolving the text
   * selection issue described in onFocus_().
   * @return Whether the <input> element was focused.
   */
  focusInput() {
    if (this.shadowRoot.activeElement === this.inputElement) {
      return false;
    }
    this.inputElement.focus();
    return true;
  }
  /**
   * 'change' event fires when <input> value changes and user presses 'Enter'.
   * This function helps propagate it to host since change events don't
   * propagate across Shadow DOM boundary by default.
   */
  async onInputChange_(e5) {
    await this.updateComplete;
    this.fire("change", { sourceEvent: e5 });
  }
  onInput_(e5) {
    this.internalValue_ = e5.target.value;
    this.value = this.internalValue_;
  }
  onInputFocus_() {
    this.focused_ = true;
  }
  onInputBlur_() {
    this.focused_ = false;
  }
  getAriaLabel_() {
    return this.ariaLabel || this.label || this.placeholder;
  }
  // Returns the id of the visible label element when aria-labelledby
  // should reference it, or null otherwise. Some accessibility frameworks
  // (notably ATK/AT-SPI used by Orca on Linux) do not reliably surface
  // the input's accessible name from aria-label alone in this layout.
  // Skipped when the host sets its own aria-label, so that takes
  // precedence as before.
  getAriaLabelledBy_() {
    if (this.label && !this.ariaLabel) {
      return "label";
    }
    return null;
  }
  // The visible label is exposed to a11y only when referenced via
  // aria-labelledby; otherwise it stays aria-hidden to avoid duplicating
  // the inner input's aria-label.
  getLabelAriaHidden_() {
    return this.getAriaLabelledBy_() ? null : "true";
  }
  getAriaInvalid_() {
    return this.invalid ? "true" : "false";
  }
  getErrorMessage_() {
    return this.invalid ? this.errorMessage : "";
  }
  getErrorRole_() {
    return this.invalid ? "alert" : "";
  }
  getAriaErrorMessage_() {
    return this.invalid ? "error" : "";
  }
  /**
   * Selects the text within the input. If no parameters are passed, it will
   * select the entire string. Either no params or both params should be passed.
   * Publicly, this function should be used instead of inputElement.select() or
   * manipulating inputElement.selectionStart/selectionEnd because the order of
   * execution between focus() and select() is sensitive.
   */
  select(start, end) {
    this.inputElement.focus();
    if (start !== void 0 && end !== void 0) {
      this.inputElement.setSelectionRange(start, end);
    } else {
      assert(start === void 0 && end === void 0);
      this.inputElement.select();
    }
  }
  // Note: In order to preserve it as a synchronous API, validate() forces 2
  // rendering updates to cr-input. This allows this function to be used to
  // synchronously determine the validity of a <cr-input>, however, as a result
  // of these 2 forced updates it may result in slower performance. validate()
  // should not be called internally from within cr_input.ts, and should only
  // be called where necessary from clients.
  validate() {
    this.performUpdate();
    this.invalid = !this.inputElement.checkValidity();
    this.performUpdate();
    return !this.invalid;
  }
};
customElements.define(CrInputElement.is, CrInputElement);
function getCss13() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_shared_vars.css.js\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-toast-background: var(--color-toast-background,\n      var(--cr-fallback-color-inverse-surface));\n  --cr-toast-button-color: var(--color-toast-button,\n      var(--cr-fallback-color-inverse-primary));\n  --cr-toast-text-color: var(--color-toast-foreground,\n      var(--cr-fallback-color-inverse-on-surface));\n  --cr-focus-outline-color: var(--cr-focus-outline-inverse-color);\n\n  align-items: center;\n  background: var(--cr-toast-background);\n  border-radius: 8px;\n  bottom: 0;\n  box-shadow: 0 2px 4px 0 rgba(0, 0, 0, 0.28);\n  box-sizing: border-box;\n  display: flex;\n  line-height: 20px;\n  margin: 24px;\n  max-width: var(--cr-toast-max-width, 568px);\n  min-height: 52px;\n  min-width: 288px;\n  opacity: 0;\n  padding: 0 16px;\n  position: fixed;\n  transform: translateY(100px);\n  transition: opacity 300ms, transform 300ms;\n  visibility: hidden;\n  z-index: 1;\n}\n\n:host-context([dir=ltr]) {\n  left: 0;\n}\n\n:host-context([dir=rtl]) {\n  right: 0;\n}\n\n:host([open]) {\n  opacity: 1;\n  transform: translateY(0);\n  visibility: visible;\n}\n\n:host(:not([open])) ::slotted(*) {\n  display: none;\n}\n\n/* Note: this doesn't work on slotted text nodes. Something like\n * <cr-toast>hey!</cr-toast> wont get the right text color. */\n:host ::slotted(*) {\n  color: var(--cr-toast-text-color);\n}\n\n:host ::slotted(cr-button) {\n  background-color: transparent !important;\n  border: none !important;\n  color: var(--cr-toast-button-color) !important;\n  margin-inline-start: 32px !important;\n  min-width: 52px !important;\n  padding: 8px !important;\n}\n\n:host ::slotted(cr-button:hover) {\n  background-color: transparent !important;\n}\n\n::slotted(cr-button:last-of-type) {\n  margin-inline-end: -8px;\n}\n"])];
}
function getHtml6() {
  return b2`<slot></slot>`;
}
var CrToastElement = class extends CrLitElement {
  static get is() {
    return "cr-toast";
  }
  static get styles() {
    return getCss13();
  }
  render() {
    return getHtml6.bind(this)();
  }
  static get properties() {
    return {
      duration: {
        type: Number
      },
      open: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #duration = 0;
  get duration() {
    return this.#duration;
  }
  set duration(_2) {
    this.#duration = _2;
  }
  #open = false;
  get open() {
    return this.#open;
  }
  set open(_2) {
    this.#open = _2;
  }
  hideTimeoutId_ = null;
  constructor() {
    super();
    this.addEventListener("focusin", this.clearTimeout_);
    this.addEventListener("focusout", this.resetAutoHide_);
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("duration") || changedProperties.has("open")) {
      this.resetAutoHide_();
    }
  }
  clearTimeout_() {
    if (this.hideTimeoutId_ !== null) {
      window.clearTimeout(this.hideTimeoutId_);
      this.hideTimeoutId_ = null;
    }
  }
  /**
   * Cancels existing auto-hide, and sets up new auto-hide.
   */
  resetAutoHide_() {
    this.clearTimeout_();
    if (this.open && this.duration !== 0) {
      this.hideTimeoutId_ = window.setTimeout(() => {
        this.hide();
      }, this.duration);
    }
  }
  /**
   * Shows the toast and auto-hides after |this.duration| milliseconds has
   * passed. If the toast is currently being shown, any preexisting auto-hide
   * is cancelled and replaced with a new auto-hide.
   */
  async show() {
    const shouldResetAutohide = this.open;
    this.removeAttribute("role");
    this.open = true;
    await this.updateComplete;
    this.setAttribute("role", "alert");
    if (shouldResetAutohide) {
      this.resetAutoHide_();
    }
  }
  /**
   * Hides the toast and ensures that its contents can not be focused while
   * hidden.
   */
  async hide() {
    this.open = false;
    await this.updateComplete;
  }
};
customElements.define(CrToastElement.is, CrToastElement);
function getCss14() {
  return [getCss3(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_hidden_style_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n#content {\n  display: flex;\n  flex: 1;\n}\n\n.collapsible {\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\nspan {\n  white-space: pre;\n}\n\n.elided-text {\n  overflow: hidden;\n  text-overflow: ellipsis;\n  white-space: var(--cr-toast-white-space, nowrap);\n}\n"])];
}
function getHtml7() {
  return b2`
<cr-toast id="toast" .duration="${this.duration}">
  <div id="content" class="elided-text"></div>
  <slot id="slotted"></slot>
</cr-toast>`;
}
var toastManagerInstance = null;
function setInstance(instance2) {
  assert(!instance2 || !toastManagerInstance);
  toastManagerInstance = instance2;
}
var CrToastManagerElement = class extends CrLitElement {
  static get is() {
    return "cr-toast-manager";
  }
  static get styles() {
    return getCss14();
  }
  render() {
    return getHtml7.bind(this)();
  }
  static get properties() {
    return {
      duration: {
        type: Number
      }
    };
  }
  #duration = 0;
  get duration() {
    return this.#duration;
  }
  set duration(_2) {
    this.#duration = _2;
  }
  get isToastOpen() {
    return this.$.toast.open;
  }
  get slottedHidden() {
    return this.$.slotted.hidden;
  }
  connectedCallback() {
    super.connectedCallback();
    setInstance(this);
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    setInstance(null);
  }
  /**
   * @param label The label to display inside the toast.
   */
  show(label, hideSlotted = false) {
    this.$.content.textContent = label;
    this.showInternal_(hideSlotted);
  }
  /**
   * Shows the toast, making certain text fragments collapsible.
   */
  showForStringPieces(pieces, hideSlotted = false) {
    const content = this.$.content;
    content.textContent = "";
    pieces.forEach(function(p4) {
      if (p4.value.length === 0) {
        return;
      }
      const span = document.createElement("span");
      span.textContent = p4.value;
      if (p4.collapsible) {
        span.classList.add("collapsible");
      }
      content.appendChild(span);
    });
    this.showInternal_(hideSlotted);
  }
  showInternal_(hideSlotted) {
    this.$.slotted.hidden = hideSlotted;
    this.$.toast.show();
  }
  hide() {
    this.$.toast.hide();
  }
};
customElements.define(CrToastManagerElement.is, CrToastManagerElement);
function getCss15() {
  return [i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  display: none;\n}\n"])];
}
function getHtml8() {
  return b2`
<svg id="baseSvg" xmlns="http://www.w3.org/2000/svg"
    viewBox="0 0 ${this.size} ${this.size}" preserveAspectRatio="xMidYMid meet"
    focusable="false">
</svg>
<slot></slot>`;
}
var APPLIED_ICON_CLASS = "cr-iconset-svg-icon_";
var CrIconsetElement = class extends CrLitElement {
  static get is() {
    return "cr-iconset";
  }
  static get styles() {
    return getCss15();
  }
  render() {
    return getHtml8.bind(this)();
  }
  static get properties() {
    return {
      /**
       * The name of the iconset.
       */
      name: { type: String },
      /**
       * The size of an individual icon. Note that icons must be square.
       */
      size: { type: Number }
    };
  }
  #name = "";
  get name() {
    return this.#name;
  }
  set name(_2) {
    this.#name = _2;
  }
  #size = 24;
  get size() {
    return this.#size;
  }
  set size(_2) {
    this.#size = _2;
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("name")) {
      assert(changedProperties.get("name") === void 0);
      IconsetMap.getInstance().set(this.name, this);
    }
  }
  /**
   * Applies an icon to the given element.
   *
   * An svg icon is prepended to the element's shadowRoot, which should always
   * exist.
   * @param element Element to which the icon is applied.
   * @param iconName Name of the icon to apply.
   * @return The svg element which renders the icon.
   */
  applyIcon(element, iconName) {
    this.removeIcon(element);
    const svg = this.cloneIcon_(iconName);
    if (svg) {
      svg.classList.add(APPLIED_ICON_CLASS);
      element.shadowRoot.insertBefore(svg, element.shadowRoot.childNodes[0]);
      return svg;
    }
    return null;
  }
  /**
   * Produce installable clone of the SVG element matching `id` in this
   * iconset, or null if there is no matching element.
   * @param iconName Name of the icon to apply.
   */
  createIcon(iconName) {
    return this.cloneIcon_(iconName);
  }
  /**
   * Remove an icon from the given element by undoing the changes effected
   * by `applyIcon`.
   */
  removeIcon(element) {
    const oldSvg = element.shadowRoot.querySelector(
      `.${APPLIED_ICON_CLASS}`
    );
    if (oldSvg) {
      oldSvg.remove();
    }
  }
  /**
   * Produce installable clone of the SVG element matching `id` in this
   * iconset, or `undefined` if there is no matching element.
   *
   * Returns an installable clone of the SVG element matching `id` or null if
   * no such element exists.
   */
  cloneIcon_(id) {
    const sourceSvg = this.querySelector(`g[id="${id}"]`);
    if (!sourceSvg) {
      return null;
    }
    const svgClone = this.$.baseSvg.cloneNode(true);
    const content = sourceSvg.cloneNode(true);
    content.removeAttribute("id");
    const contentViewBox = content.getAttribute("viewBox");
    if (contentViewBox) {
      svgClone.setAttribute("viewBox", contentViewBox);
    }
    svgClone.style.display = "block";
    svgClone.style.height = "100%";
    svgClone.style.width = "100%";
    svgClone.style.pointerEvents = "none";
    svgClone.appendChild(content);
    return svgClone;
  }
};
customElements.define(CrIconsetElement.is, CrIconsetElement);
function isValidArray(arr) {
  if (arr instanceof Array && Object.isFrozen(arr)) {
    return true;
  }
  return false;
}
function getStaticString(literal) {
  const isStaticString = isValidArray(literal) && !!literal.raw && isValidArray(literal.raw) && literal.length === literal.raw.length && literal.length === 1;
  assert(isStaticString, "static_types.js only allows static strings");
  return literal.join("");
}
function createTypes(_ignore, literal) {
  return getStaticString(literal);
}
var rules = {
  createHTML: createTypes,
  createScript: createTypes,
  createScriptURL: createTypes
};
var staticPolicy;
if (window.trustedTypes) {
  staticPolicy = window.trustedTypes.createPolicy("static-types", rules);
} else {
  staticPolicy = rules;
}
function getTrustedHTML(literal) {
  return staticPolicy.createHTML("", literal);
}
var div = document.createElement("div");
if (document.documentElement.hasAttribute("webui-rounded-icons")) {
  div.innerHTML = getTrustedHTML`
<cr-iconset name="cr20" size="20">
  <svg>
    <defs>
      <!--
      Keep these in sorted order by id="".
      -->
      <g id="block">
        <path d="M6.895 17.375a8.068 8.068 0 0 1-2.551-1.719 8.068 8.068 0 0 1-1.719-2.55A7.78 7.78 0 0 1 2 10c0-1.11.207-2.148.625-3.113a8.122 8.122 0 0 1 1.719-2.543c.73-.73 1.578-1.301 2.55-1.719A7.81 7.81 0 0 1 10.013 2a7.72 7.72 0 0 1 3.101.625 8.122 8.122 0 0 1 2.543 1.719c.73.73 1.301 1.578 1.719 2.543A7.75 7.75 0 0 1 18 10a7.78 7.78 0 0 1-.625 3.105 8.068 8.068 0 0 1-1.719 2.551c-.73.73-1.578 1.301-2.543 1.719a7.72 7.72 0 0 1-3.101.625 7.81 7.81 0 0 1-3.117-.625ZM10 16.5c.766 0 1.484-.125 2.168-.375a6.545 6.545 0 0 0 1.852-1.043L4.918 5.98a6.545 6.545 0 0 0-1.043 1.852A6.246 6.246 0 0 0 3.5 10c0 1.805.633 3.34 1.895 4.605C6.66 15.867 8.195 16.5 10 16.5Zm5.082-2.48a6.545 6.545 0 0 0 1.043-1.852c.25-.684.375-1.402.375-2.168 0-1.805-.633-3.34-1.895-4.605C13.34 4.133 11.805 3.5 10 3.5c-.766 0-1.484.125-2.168.375-.68.25-1.297.598-1.852 1.043ZM10 10Zm0 0"></path>
      </g>
      <g id="cloud-off">
        <path d="M5 16c-1.113 0-2.063-.387-2.836-1.164C1.387 14.062 1 13.113 1 12a3.84 3.84 0 0 1 1.082-2.73A3.895 3.895 0 0 1 4.73 8.02c.047-.149.09-.293.137-.434.047-.14.106-.281.176-.418L2.395 4.52a.725.725 0 0 1-.227-.532c0-.199.074-.379.227-.531a.73.73 0 0 1 .53-.227c.204 0 .38.075.532.227l13.086 13.086a.712.712 0 0 1 .227.52.712.712 0 0 1-.227.519.729.729 0 0 1-1.063 0L13.875 16Zm0-1.5h7.375L6.207 8.332c-.055.184-.105.367-.156.555a8.152 8.152 0 0 1-.156.55l-1.063.083c-.664.058-1.223.32-1.664.785A2.375 2.375 0 0 0 2.5 12c0 .691.242 1.281.73 1.77A2.41 2.41 0 0 0 5 14.5Zm4.293-3.082Zm8.207 3.957-1.105-1.082c.32-.168.585-.41.793-.727.207-.316.312-.671.312-1.066 0-.559-.191-1.035-.578-1.422A1.936 1.936 0 0 0 15.5 10.5h-1.375L14 9.145a3.91 3.91 0 0 0-1.293-2.594A3.88 3.88 0 0 0 9.997 5.5c-.34 0-.669.043-.981.125a4.233 4.233 0 0 0-.91.355L7.02 4.895a6.34 6.34 0 0 1 1.41-.657A5.08 5.08 0 0 1 10 4c1.43 0 2.672.484 3.73 1.45 1.055.964 1.645 2.148 1.77 3.55.973 0 1.797.34 2.48 1.02.68.683 1.02 1.507 1.02 2.48 0 .598-.137 1.145-.406 1.645-.27.5-.637.91-1.094 1.23Zm-5.25-5.25Zm0 0"></path>
      </g>
      <g id="delete">
        <path d="M6.5 17c-.414 0-.766-.148-1.059-.441A1.443 1.443 0 0 1 5 15.5v-10h-.25c-.21 0-.39-.07-.535-.215A.713.713 0 0 1 4 4.754c0-.211.07-.39.215-.535A.73.73 0 0 1 4.75 4H8v-.25c0-.21.07-.39.215-.535A.727.727 0 0 1 8.75 3h2.5c.21 0 .39.07.535.215A.727.727 0 0 1 12 3.75V4h3.25c.21 0 .39.07.535.215.145.14.215.32.215.531 0 .211-.07.39-.215.535a.73.73 0 0 1-.535.219H15v9.992c0 .422-.148.778-.441 1.07A1.439 1.439 0 0 1 13.5 17Zm7-11.5h-7v10h7Zm-4.219 8.285a.73.73 0 0 0 .219-.535v-5.5c0-.21-.07-.39-.215-.535A.713.713 0 0 0 8.754 7c-.211 0-.39.07-.535.215A.73.73 0 0 0 8 7.75v5.5c0 .21.07.39.215.535.14.145.32.215.531.215.211 0 .39-.07.535-.215Zm2.5 0A.73.73 0 0 0 12 13.25v-5.5c0-.21-.07-.39-.215-.535A.713.713 0 0 0 11.254 7c-.211 0-.39.07-.535.215a.73.73 0 0 0-.219.535v5.5c0 .21.07.39.215.535.14.145.32.215.531.215.211 0 .39-.07.535-.215ZM6.5 5.5v10Zm0 0"></path>
      </g>
      <g id="domain">
        <path d="M2 15.5v-11c0-.414.148-.766.441-1.059A1.443 1.443 0 0 1 3.5 3h5c.414 0 .766.148 1.059.441.293.293.441.645.441 1.059V6h6.5c.414 0 .766.148 1.059.441.293.293.441.645.441 1.059v8c0 .414-.148.766-.441 1.059A1.443 1.443 0 0 1 16.5 17h-13c-.414 0-.766-.148-1.059-.441A1.443 1.443 0 0 1 2 15.5Zm1.5 0H5V14H3.5Zm0-3.168H5v-1.5H3.5Zm0-3.164H5v-1.5H3.5ZM3.5 6H5V4.5H3.5ZM7 15.5h1.5V14H7Zm0-3.168h1.5v-1.5H7Zm0-3.164h1.5v-1.5H7ZM7 6h1.5V4.5H7Zm3 9.5h6.5v-8H10v1.668h1.5v1.5H10v1.664h1.5v1.5H10Zm3.5-4.832v-1.5H15v1.5Zm0 3.164v-1.5H15v1.5Zm0 0"></path>
      </g>
      <g id="family-link">
        <path d="M11.105 15.293 16 7.688 11.082 2.5 6.02 7.688Zm-.085-6.398ZM8.875 19a2.49 2.49 0 0 1-1.762-.7 3.291 3.291 0 0 1-.968-1.718 1.267 1.267 0 0 0-.442-.695 1.167 1.167 0 0 0-.766-.282c-.207 0-.402.04-.582.125a.889.889 0 0 0-.418.395c-.07.14-.164.258-.285.355a.65.65 0 0 1-.422.145.742.742 0 0 1-.542-.23.74.74 0 0 1-.231-.54c0-.09.016-.168.043-.234.027-.066.063-.137.105-.203a2.83 2.83 0 0 1 1-.969 2.603 2.603 0 0 1 1.352-.367c.684 0 1.262.223 1.742.668.477.445.793 1.008.946 1.688.07.304.214.558.43.761.214.2.48.301.8.301.234 0 .477-.07.719-.207.242-.14.43-.309.554-.504l.02-.039L4.75 8.52a1.27 1.27 0 0 1-.191-.395 1.66 1.66 0 0 1 .035-.984c.062-.176.172-.344.324-.496L10 1.457A1.55 1.55 0 0 1 11.09 1a1.526 1.526 0 0 1 1.078.457l4.914 5.188c.14.148.246.312.313.488a1.584 1.584 0 0 1 .047.992 1.27 1.27 0 0 1-.192.395l-5.98 9.062c-.348.527-.723.898-1.133 1.105A2.752 2.752 0 0 1 8.875 19Zm0 0"></path>
      </g>
      <g id="menu">
        <path d="M3.75 14.5c-.21 0-.39-.07-.535-.215A.713.713 0 0 1 3 13.754c0-.211.07-.39.215-.535A.73.73 0 0 1 3.75 13h12.5c.21 0 .39.07.535.215.145.14.215.32.215.531 0 .211-.07.39-.215.535a.73.73 0 0 1-.535.219Zm0-3.75c-.21 0-.39-.07-.535-.215A.713.713 0 0 1 3 10.004c0-.211.07-.39.215-.535a.73.73 0 0 1 .535-.219h12.5c.21 0 .39.07.535.215.145.14.215.32.215.531 0 .211-.07.39-.215.535a.73.73 0 0 1-.535.219Zm0-3.75c-.21 0-.39-.07-.535-.215A.713.713 0 0 1 3 6.254c0-.211.07-.39.215-.535A.73.73 0 0 1 3.75 5.5h12.5c.21 0 .39.07.535.215.145.14.215.32.215.531 0 .211-.07.39-.215.535A.73.73 0 0 1 16.25 7Zm0 0"></path>
      </g>
      <g id="password-manager">
        <path d="M7.418 11.418C7.805 11.028 8 10.555 8 10c0-.555-.195-1.027-.582-1.418A1.943 1.943 0 0 0 6 8c-.555 0-1.027.195-1.418.582C4.195 8.972 4 9.445 4 10c0 .555.195 1.027.582 1.418.39.387.863.582 1.418.582.555 0 1.027-.195 1.418-.582ZM6 15c-1.39 0-2.57-.484-3.543-1.457C1.484 12.57 1 11.391 1 10c0-1.39.484-2.57 1.457-3.543C3.43 5.484 4.609 5 6 5c1 0 1.91.27 2.73.813A5.086 5.086 0 0 1 10.582 8H17.5c.414 0 .766.148 1.059.441.293.293.441.645.441 1.059v4c0 .414-.148.766-.441 1.059A1.443 1.443 0 0 1 17.5 15h-3c-.414 0-.766-.148-1.059-.441A1.443 1.443 0 0 1 13 13.5V12h-2.418a5.086 5.086 0 0 1-1.852 2.188A4.84 4.84 0 0 1 6 15Zm3.457-4.5H14.5v3h1V12c0-.145.047-.266.145-.36A.492.492 0 0 1 16 11.5c.14 0 .258.047.355.14a.477.477 0 0 1 .145.36v1.5h1v-4H9.457a3.27 3.27 0 0 0-1.156-2.156A3.452 3.452 0 0 0 6 6.5c-.973 0-1.797.34-2.48 1.02C2.84 8.203 2.5 9.027 2.5 10c0 .973.34 1.797 1.02 2.48.683.68 1.507 1.02 2.48 1.02.871 0 1.637-.281 2.3-.844A3.27 3.27 0 0 0 9.458 10.5Zm0 0"></path>
      </g>
      
  </svg>
</cr-iconset>

<!-- NOTE: In the common case that the final icon will be 20x20, export the SVG
     at 20px and place it in the section above. -->
<cr-iconset name="cr" size="24">
  <svg>
    <defs>
      <!--
      These icons are copied from Polymer's iron-icons and kept in sorted order.
      -->
      <g id="add">
        <path d="M11.102 12.898H6.898A.872.872 0 0 1 6 12.003c0-.253.086-.464.258-.64a.86.86 0 0 1 .64-.261h4.204V6.898A.872.872 0 0 1 11.997 6c.253 0 .464.086.64.258a.86.86 0 0 1 .261.64v4.204h4.204a.872.872 0 0 1 .898.895.878.878 0 0 1-.258.64.86.86 0 0 1-.64.261h-4.204v4.204a.872.872 0 0 1-.895.898.878.878 0 0 1-.64-.258.86.86 0 0 1-.261-.64Zm0 0"></path>
      </g>
      <g id="arrow-back">
        <path d="m8.25 12.898 4.406 4.407c.18.18.27.39.27.633a.913.913 0 0 1-.278.648.893.893 0 0 1-.636.266.855.855 0 0 1-.633-.274l-5.945-5.95a.894.894 0 0 1-.196-.292.891.891 0 0 1-.062-.34c0-.121.02-.234.062-.34.04-.105.102-.199.188-.281l5.949-5.95a.883.883 0 0 1 .64-.277c.247 0 .458.094.633.278a.847.847 0 0 1 .278.633c0 .246-.09.457-.274.636L8.25 11.102H18.3a.872.872 0 0 1 .898.895.878.878 0 0 1-.257.64.86.86 0 0 1-.64.261Zm0 0"></path>
      </g>
      <g id="arrow-drop-up">
        <path d="M8.273 14.398a.419.419 0 0 1-.324-.132.456.456 0 0 1-.125-.317c0-.015.051-.12.149-.32l3.55-3.555a.651.651 0 0 1 .227-.148.636.636 0 0 1 .5 0c.082.031.16.082.227.148l3.55 3.555a.47.47 0 0 1 .11.16c.027.059.039.113.039.172a.444.444 0 0 1-.125.309.43.43 0 0 1-.324.128Zm0 0"></path>
      </g>
      <g id="arrow-drop-down">
        <path d="m11.523 13.926-3.55-3.555a.47.47 0 0 1-.11-.16.393.393 0 0 1-.039-.172c0-.117.043-.219.125-.309a.43.43 0 0 1 .324-.128h7.454c.132 0 .242.043.324.132.082.09.125.196.125.317 0 .015-.051.12-.149.32l-3.55 3.555a.651.651 0 0 1-.227.148.636.636 0 0 1-.5 0 .651.651 0 0 1-.227-.148Zm0 0"></path>
      </g>
      <g id="arrow-forward">
        <path d="M15.75 12.898H5.7a.872.872 0 0 1-.898-.895c-.001-.253.085-.464.257-.64a.86.86 0 0 1 .64-.261H15.75l-4.406-4.407a.866.866 0 0 1-.27-.633c0-.242.094-.457.278-.648a.893.893 0 0 1 .636-.266c.242 0 .453.09.633.274l5.945 5.95c.09.093.157.19.196.292a.891.891 0 0 1 .062.34c0 .121-.02.234-.062.34a.742.742 0 0 1-.188.281l-5.949 5.95c-.184.183-.39.273-.625.26a.923.923 0 0 1-.625-.273.918.918 0 0 1-.273-.652c0-.246.09-.457.273-.637Zm0 0"></path>
      </g>
      <g id="arrow-right">
        <path d="M10.04 16.176a.444.444 0 0 1-.31-.125.43.43 0 0 1-.128-.324V8.273c0-.132.043-.242.132-.324a.456.456 0 0 1 .317-.125c.015 0 .12.051.32.149l3.555 3.55a.651.651 0 0 1 .148.227.636.636 0 0 1 0 .5.651.651 0 0 1-.148.227l-3.555 3.55a.47.47 0 0 1-.16.11.393.393 0 0 1-.172.039Zm0 0"></path>
      </g>
      <g id="cancel-filled">
        <path d="m12 13.273 2.898 2.903c.184.183.399.27.641.262a.926.926 0 0 0 .637-.29.87.87 0 0 0 .273-.636.87.87 0 0 0-.273-.637L13.273 12l2.903-2.898a.879.879 0 0 0 0-1.278.879.879 0 0 0-1.277 0L12 10.727 9.102 7.824a.864.864 0 0 0-.625-.273.857.857 0 0 0-.625.273.88.88 0 0 0-.278.637c0 .242.094.457.278.64L10.727 12l-2.903 2.898a.822.822 0 0 0-.261.625.907.907 0 0 0 .289.625.874.874 0 0 0 .636.278.874.874 0 0 0 .637-.278Zm0 8.329a9.369 9.369 0 0 1-3.727-.75 9.706 9.706 0 0 1-3.062-2.063 9.706 9.706 0 0 1-2.063-3.062A9.369 9.369 0 0 1 2.398 12c0-1.332.25-2.578.75-3.738A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063c1.168-.5 2.41-.75 3.727-.75 1.332 0 2.578.25 3.738.75a9.743 9.743 0 0 1 3.051 2.063 9.743 9.743 0 0 1 2.063 3.05c.5 1.16.75 2.407.75 3.739 0 1.316-.25 2.559-.75 3.727a9.706 9.706 0 0 1-2.063 3.062 9.743 9.743 0 0 1-3.05 2.063c-1.16.5-2.407.75-3.739.75Zm0 0"></path>
      </g>
      <g id="check">
        <path d="m9.727 14.773 7.472-7.472a.88.88 0 0 1 .637-.278c.242 0 .457.094.64.278a.87.87 0 0 1 .274.636.87.87 0 0 1-.273.637L10.375 16.7a.874.874 0 0 1-.637.278.874.874 0 0 1-.636-.278l-3.579-3.574a.87.87 0 0 1-.273-.637.87.87 0 0 1 .273-.636.883.883 0 0 1 .641-.278.88.88 0 0 1 .637.278Zm0 0"></path>
      </g>
      <g id="check-circle">
        <path d="m10.727 13.05-1.5-1.476a.864.864 0 0 0-.625-.273.857.857 0 0 0-.625.273.88.88 0 0 0-.278.637c0 .242.094.457.278.64l2.125 2.126c.183.183.39.273.625.273.23 0 .441-.09.625-.273l4.671-4.676a.88.88 0 0 0 .278-.637.883.883 0 0 0-.278-.64.857.857 0 0 0-.625-.274c-.23 0-.441.09-.625.273ZM12 21.603a9.369 9.369 0 0 1-3.727-.75 9.706 9.706 0 0 1-3.062-2.063 9.706 9.706 0 0 1-2.063-3.062A9.369 9.369 0 0 1 2.398 12c0-1.332.25-2.578.75-3.738A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063c1.168-.5 2.41-.75 3.727-.75 1.332 0 2.578.25 3.738.75a9.743 9.743 0 0 1 3.051 2.063 9.743 9.743 0 0 1 2.063 3.05c.5 1.16.75 2.407.75 3.739 0 1.316-.25 2.559-.75 3.727a9.706 9.706 0 0 1-2.063 3.062 9.743 9.743 0 0 1-3.05 2.063c-1.16.5-2.407.75-3.739.75Zm0-1.801c2.168 0 4.008-.758 5.523-2.278 1.52-1.515 2.278-3.355 2.278-5.523 0-2.168-.758-4.008-2.278-5.523-1.515-1.52-3.355-2.278-5.523-2.278-2.168 0-4.008.758-5.523 2.278C4.957 7.992 4.199 9.832 4.199 12c0 2.168.758 4.008 2.278 5.523 1.515 1.52 3.355 2.278 5.523 2.278ZM12 12Zm0 0"></path>
      </g>
      <g id="chevron-left">
        <path d="m10.95 12 4.1 4.102a.87.87 0 0 1 .274.636.87.87 0 0 1-.273.637.879.879 0 0 1-1.277 0l-4.75-4.75a.84.84 0 0 1-.188-.29.967.967 0 0 1-.063-.335c0-.117.024-.23.063-.336a.84.84 0 0 1 .187-.289l4.75-4.75a.879.879 0 0 1 1.278 0 .87.87 0 0 1 .273.637.87.87 0 0 1-.273.636Zm0 0"></path>
      </g>
      <g id="chevron-right">
        <path d="m13.05 12-4.1-4.102a.87.87 0 0 1-.274-.636.87.87 0 0 1 .273-.637.879.879 0 0 1 1.277 0l4.75 4.75a.84.84 0 0 1 .188.29c.04.105.063.218.063.335 0 .117-.024.23-.063.336a.84.84 0 0 1-.187.289l-4.75 4.75a.822.822 0 0 1-.625.262.902.902 0 0 1-.625-.285.883.883 0 0 1-.278-.641.88.88 0 0 1 .278-.637Zm0 0"></path>
      </g>
      <g id="chrome-product">
        <path d="M9.45 14.574c.698.7 1.55 1.051 2.55 1.051 1 0 1.852-.352 2.55-1.05.7-.7 1.052-1.552 1.052-2.552s-.352-1.847-1.051-2.546C13.85 8.773 13 8.426 12 8.426c-1 0-1.852.347-2.55 1.05-.7.7-1.052 1.547-1.052 2.547 0 1 .352 1.852 1.051 2.551ZM12 17.426c.184 0 .371-.004.563-.012.19-.012.378-.039.562-.09l-2.45 4.2c-2.402-.266-4.378-1.31-5.937-3.126-1.558-1.816-2.34-3.941-2.34-6.375 0-.714.082-1.418.239-2.109.16-.691.398-1.371.715-2.039l4 6.852A4.988 4.988 0 0 0 9.3 16.71a5.346 5.346 0 0 0 2.699.715Zm0-10.801a5.114 5.114 0 0 0-3.164 1.05 5.499 5.499 0 0 0-1.938 2.7L4.45 6.125a8.93 8.93 0 0 1 3.313-2.738A9.61 9.61 0 0 1 12 2.426c1.582 0 3.074.375 4.477 1.125a9.739 9.739 0 0 1 3.449 3.074Zm8.898 1.8c.25.583.43 1.177.54 1.774.109.602.164 1.211.164 1.824 0 2.586-.836 4.754-2.5 6.516-1.668 1.758-3.793 2.777-6.375 3.063l3.921-6.875c.235-.418.418-.852.551-1.301a5.058 5.058 0 0 0-.176-3.324A5.943 5.943 0 0 0 16 8.426Zm0 0"></path>
      </g>
      <g id="close">
        <path d="m12 13.273-4.102 4.102a.816.816 0 0 1-.625.262.91.91 0 0 1-.625-.285.879.879 0 0 1 0-1.278L10.727 12 6.625 7.898a.83.83 0 0 1-.262-.636.914.914 0 0 1 .285-.637.879.879 0 0 1 1.278 0L12 10.727l4.102-4.102a.87.87 0 0 1 .636-.273.87.87 0 0 1 .637.273.87.87 0 0 1 .273.637.87.87 0 0 1-.273.636L13.273 12l4.102 4.102c.184.183.273.39.273.625 0 .23-.09.441-.273.625a.87.87 0 0 1-.637.273.87.87 0 0 1-.636-.273Zm0 0"></path>
      </g>
      <g id="computer">
        <path d="M2.102 20.398a.872.872 0 0 1-.902-.895c0-.253.085-.464.257-.64a.869.869 0 0 1 .645-.261h19.796a.872.872 0 0 1 .902.895.878.878 0 0 1-.257.64.869.869 0 0 1-.645.261Zm2.097-3c-.496 0-.918-.175-1.27-.527a1.738 1.738 0 0 1-.53-1.27V5.399c0-.492.18-.918.53-1.27a1.729 1.729 0 0 1 1.27-.526h15.602c.496 0 .918.175 1.27.527.35.351.53.777.53 1.27v10.203c0 .492-.18.918-.53 1.27a1.729 1.729 0 0 1-1.27.526Zm0-1.796h15.602V5.398H4.199Zm0 0V5.398Zm0 0"></path>
      </g>
      <g id="edit-filled">
        <path d="M4.504 20.398a.878.878 0 0 1-.64-.257.846.846 0 0 1-.262-.637v-2.172c0-.238.039-.465.125-.684.082-.214.214-.414.398-.597L16.051 4.125a1.728 1.728 0 0 1 1.27-.523c.238 0 .464.039.679.125.215.082.418.214.602.398l1.273 1.273a1.773 1.773 0 0 1 .523 1.266c0 .242-.039.469-.125.688a1.639 1.639 0 0 1-.398.597L7.949 19.875a1.706 1.706 0 0 1-.597.398 1.81 1.81 0 0 1-.68.125ZM17.324 7.95l1.278-1.273-1.278-1.278-1.273 1.278Zm0 0"></path>
      </g>
      <g id="delete">
        <path d="M7.8 20.398c-.495 0-.917-.175-1.273-.527A1.735 1.735 0 0 1 6 18.601v-12h-.3a.872.872 0 0 1-.641-.257.87.87 0 0 1-.258-.637c0-.254.086-.469.258-.644a.86.86 0 0 1 .64-.262h3.903V4.5c0-.254.085-.469.257-.64a.872.872 0 0 1 .641-.258h3c.254 0 .469.086.64.257a.872.872 0 0 1 .258.641v.3h3.903c.254 0 .469.087.64.259a.863.863 0 0 1 .258.636.872.872 0 0 1-.258.64.864.864 0 0 1-.64.267H18V18.59c0 .508-.176.933-.527 1.285a1.74 1.74 0 0 1-1.274.523Zm8.4-13.796H7.8v12h8.4Zm-5.063 9.941a.869.869 0 0 0 .261-.645V9.301a.884.884 0 0 0-.894-.903.866.866 0 0 0-.64.262.86.86 0 0 0-.262.64v6.598a.872.872 0 0 0 .895.902.878.878 0 0 0 .64-.257Zm3 0a.869.869 0 0 0 .261-.645V9.301a.884.884 0 0 0-.894-.903.866.866 0 0 0-.64.262.86.86 0 0 0-.262.64v6.598a.872.872 0 0 0 .895.902.878.878 0 0 0 .64-.257ZM7.8 6.602v12Zm0 0"></path>
      </g>
      <g id="devices">
        <path d="M12.602 10.5Zm-.903 8.7H3.301a.872.872 0 0 1-.64-.259.852.852 0 0 1-.263-.636.86.86 0 0 1 .262-.64.864.864 0 0 1 .64-.267h8.4c.253 0 .468.086.64.258a.858.858 0 0 1 .262.637.875.875 0 0 1-.262.645.86.86 0 0 1-.64.261Zm-6.3-3c-.547 0-.985-.16-1.313-.49-.324-.323-.484-.76-.484-1.312V6.602c0-.551.16-.989.484-1.313.328-.328.766-.488 1.312-.488H19.5c.254 0 .469.086.64.258a.863.863 0 0 1 .258.636.872.872 0 0 1-.257.64.864.864 0 0 1-.641.267H5.398v7.796H11.7c.254 0 .469.086.64.258a.858.858 0 0 1 .263.637.875.875 0 0 1-.262.645.86.86 0 0 1-.64.261ZM19.8 17.397V10.2h-3.602v7.2ZM15.75 19.2c-.418 0-.746-.12-.988-.363-.242-.238-.364-.57-.364-.984V9.75c0-.418.122-.746.364-.988s.57-.364.988-.364h4.5c.418 0 .746.122.988.364s.364.57.364.988v8.102c0 .414-.122.746-.364.984-.242.242-.57.363-.988.363ZM17.992 12a.578.578 0 0 0 .434-.18.564.564 0 0 0 .176-.414.592.592 0 0 0-.61-.605.57.57 0 0 0-.418.172.577.577 0 0 0-.176.433c0 .164.063.301.18.418a.57.57 0 0 0 .414.176ZM18 13.8Zm0 0"></path>
      </g>
      <g id="domain">
        <path d="M2.398 18.602V5.398c0-.492.18-.918.532-1.27a1.729 1.729 0 0 1 1.27-.526h6c.495 0 .917.175 1.273.527.351.351.527.777.527 1.27v1.8h7.8c.497 0 .919.176 1.27.531.352.352.532.774.532 1.27v9.602c0 .492-.18.918-.532 1.27a1.729 1.729 0 0 1-1.27.526H4.2c-.497 0-.919-.175-1.27-.527a1.738 1.738 0 0 1-.532-1.27Zm1.801 0H6V16.8H4.2Zm0-3.801H6V13H4.2ZM4.2 11H6V9.2H4.2Zm0-3.8H6V5.397H4.2Zm4.2 11.402h1.8V16.8H8.4Zm0-3.801h1.8V13H8.4Zm0-3.801h1.8V9.2H8.4Zm0-3.8h1.8V5.397H8.4ZM12 18.601h7.8V9H12v2h1.8v1.8H12v2h1.8v1.802H12Zm4.2-5.801V11H18v1.8Zm0 3.8v-1.8H18v1.8Zm0 0"></path>
      </g>
      <g id="download">
        <path d="M11.656 15.164a.845.845 0 0 1-.281-.187l-3.55-3.551a.83.83 0 0 1-.263-.637.949.949 0 0 1 .278-.64.909.909 0 0 1 .648-.274.87.87 0 0 1 .637.273l1.977 2V4.5a.872.872 0 0 1 .895-.898c.253 0 .464.086.64.257a.86.86 0 0 1 .261.641v7.648l2-2c.18-.183.391-.27.637-.261a.97.97 0 0 1 .652.289.89.89 0 0 1-.011 1.273l-3.551 3.528a.983.983 0 0 1-.293.187.967.967 0 0 1-.336.063.972.972 0 0 1-.34-.063ZM6.594 19.2c-.496 0-.918-.176-1.27-.527a1.749 1.749 0 0 1-.523-1.274V16.5c0-.254.086-.469.258-.64a.863.863 0 0 1 .636-.258c.254 0 .47.085.64.257a.864.864 0 0 1 .267.641v.898h10.796V16.5c0-.254.086-.469.258-.64a.87.87 0 0 1 .637-.258c.254 0 .469.085.645.257a.86.86 0 0 1 .261.641v.898c0 .497-.176.922-.527 1.274a1.744 1.744 0 0 1-1.274.527Zm0 0"></path>
      </g>
      <!-- source: https://fonts.google.com/icons?selected=Material+Symbols+Outlined:family_link:FILL@0;wght@0;GRAD@0;opsz@24&icon.size=24&icon.color=%23e8eaed -->
      <g id="family-link">
        <path d="M13.324 18.352 19.2 9.227 13.301 3 7.227 9.227Zm-.097-7.676ZM10.648 22.8c-.816 0-1.52-.281-2.109-.84a3.954 3.954 0 0 1-1.164-2.063 1.562 1.562 0 0 0-.531-.835 1.389 1.389 0 0 0-.918-.336c-.25 0-.48.046-.7.148a1.039 1.039 0 0 0-.5.477c-.085.164-.199.308-.343.421a.787.787 0 0 1-.508.176.883.883 0 0 1-.648-.273.896.896 0 0 1-.278-.653c0-.105.016-.199.051-.277.035-.082.074-.164.125-.246.3-.484.7-.871 1.2-1.164.5-.29 1.042-.438 1.624-.438.817 0 1.512.27 2.09.801.574.535.953 1.211 1.137 2.028.082.363.254.668.511.91.258.242.579.363.961.363.286 0 .575-.082.864-.25a1.9 1.9 0 0 0 .668-.602l.02-.046-6.5-9.875a1.625 1.625 0 0 1-.231-.477 1.881 1.881 0 0 1-.07-.512c0-.234.038-.457.113-.672.074-.21.203-.406.386-.59L12 1.75c.184-.184.39-.32.617-.414.23-.09.461-.137.688-.137.23 0 .457.047.683.137.223.094.43.23.614.414L20.5 7.977c.168.175.293.37.375.585a1.91 1.91 0 0 1 .055 1.188 1.533 1.533 0 0 1-.23.477l-7.177 10.875c-.414.632-.867 1.074-1.355 1.324a3.31 3.31 0 0 1-1.52.375Zm0 0"></path>
      </g>
      <g id="error-filled">
        <path d="M12.637 16.543a.852.852 0 0 0 .261-.637.9.9 0 0 0-.253-.644.86.86 0 0 0-.641-.262.878.878 0 0 0-.64.258.852.852 0 0 0-.262.637.9.9 0 0 0 .253.644.875.875 0 0 0 .641.262.878.878 0 0 0 .64-.258Zm0-3.602a.86.86 0 0 0 .261-.64V8.1a.872.872 0 0 0-.895-.902.878.878 0 0 0-.64.258.869.869 0 0 0-.261.645V12.3a.872.872 0 0 0 .895.898.878.878 0 0 0 .64-.258Zm-.63 8.66c-1.32 0-2.566-.25-3.734-.75a9.706 9.706 0 0 1-3.062-2.062 9.706 9.706 0 0 1-2.063-3.062 9.398 9.398 0 0 1-.75-3.739c0-1.324.25-2.566.75-3.726A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063 9.398 9.398 0 0 1 3.739-.75c1.324 0 2.566.25 3.726.75a9.743 9.743 0 0 1 3.051 2.063 9.688 9.688 0 0 1 2.063 3.059c.5 1.16.75 2.402.75 3.722 0 1.32-.25 2.567-.75 3.735a9.706 9.706 0 0 1-2.063 3.062 9.688 9.688 0 0 1-3.059 2.063 9.31 9.31 0 0 1-3.722.75Zm0 0"></path>
      </g>
      <g id="error">
        <path d="M12.637 16.543a.852.852 0 0 0 .261-.637.9.9 0 0 0-.253-.644.86.86 0 0 0-.641-.262.878.878 0 0 0-.64.258.852.852 0 0 0-.262.637.9.9 0 0 0 .253.644.875.875 0 0 0 .641.262.878.878 0 0 0 .64-.258Zm0-3.602a.86.86 0 0 0 .261-.64V8.1a.872.872 0 0 0-.895-.902.878.878 0 0 0-.64.258.869.869 0 0 0-.261.645V12.3a.872.872 0 0 0 .895.898.878.878 0 0 0 .64-.258Zm-.63 8.66c-1.32 0-2.566-.25-3.734-.75a9.706 9.706 0 0 1-3.062-2.062 9.706 9.706 0 0 1-2.063-3.062 9.398 9.398 0 0 1-.75-3.739c0-1.324.25-2.566.75-3.726A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063 9.398 9.398 0 0 1 3.739-.75c1.324 0 2.566.25 3.726.75a9.743 9.743 0 0 1 3.051 2.063 9.688 9.688 0 0 1 2.063 3.059c.5 1.16.75 2.402.75 3.722 0 1.32-.25 2.567-.75 3.735a9.706 9.706 0 0 1-2.063 3.062 9.688 9.688 0 0 1-3.059 2.063 9.31 9.31 0 0 1-3.722.75Zm-.007-1.8c2.168 0 4.008-.758 5.523-2.278 1.52-1.515 2.278-3.355 2.278-5.523 0-2.168-.758-4.008-2.278-5.523-1.515-1.52-3.355-2.278-5.523-2.278-2.168 0-4.008.758-5.523 2.278C4.957 7.992 4.199 9.832 4.199 12c0 2.168.758 4.008 2.278 5.523 1.515 1.52 3.355 2.278 5.523 2.278ZM12 12Zm0 0"></path>
      </g>
      <g id="keyboard-arrow-up">
        <path d="m12 10.875-4.102 4.102a.87.87 0 0 1-.636.273.87.87 0 0 1-.637-.273.879.879 0 0 1 0-1.278l4.75-4.75A.857.857 0 0 1 12 8.676c.234 0 .441.09.625.273l4.75 4.75c.184.184.273.39.273.625s-.09.442-.273.625a.874.874 0 0 1-.637.278.874.874 0 0 1-.636-.278Zm0 0"></path>
      </g>
      <g id="keyboard-arrow-down">
        <path d="M11.664 15.238a.84.84 0 0 1-.289-.187l-4.75-4.75a.835.835 0 0 1-.262-.637.923.923 0 0 1 .285-.64.879.879 0 0 1 1.278 0L12 13.124l4.102-4.102a.83.83 0 0 1 .636-.261.919.919 0 0 1 .637.289.87.87 0 0 1 .273.636.87.87 0 0 1-.273.637l-4.75 4.727a.84.84 0 0 1-.29.187.967.967 0 0 1-.335.063.967.967 0 0 1-.336-.063Zm0 0"></path>
      </g>
      <g id="chrome-extension-filled">
        <path d="M5.398 20.398c-.492 0-.918-.175-1.27-.527a1.735 1.735 0 0 1-.526-1.27v-3.476c0-.219.066-.41.199-.578a.79.79 0 0 1 .523-.297 2.746 2.746 0 0 0 1.2-.875C5.84 12.977 6 12.515 6 12c0-.516-.16-.977-.477-1.375a2.746 2.746 0 0 0-1.199-.875.8.8 0 0 1-.523-.305.947.947 0 0 1-.2-.593V5.398c0-.492.176-.918.528-1.27a1.735 1.735 0 0 1 1.27-.526h4.203c0-.668.23-1.235.691-1.704a2.317 2.317 0 0 1 1.703-.699c.668 0 1.238.235 1.703.696.469.464.7 1.035.7 1.707h4.203c.492 0 .918.175 1.27.527.35.351.526.777.526 1.27v4.203c.668 0 1.235.23 1.704.691.464.465.699 1.031.699 1.703 0 .668-.235 1.238-.696 1.703-.464.469-1.035.7-1.707.7v4.203c0 .492-.175.918-.527 1.27a1.735 1.735 0 0 1-1.27.526Zm0 0"></path>
      </g>
      <g id="fullscreen">
        <path d="M5.398 18.602H7.5a.872.872 0 0 1 .898.895.878.878 0 0 1-.257.64.86.86 0 0 1-.641.261h-3a.872.872 0 0 1-.64-.257.872.872 0 0 1-.258-.641v-3a.872.872 0 0 1 .895-.898c.253 0 .464.085.64.257a.86.86 0 0 1 .261.641Zm13.204 0V16.5a.872.872 0 0 1 .895-.898c.253 0 .464.085.64.257a.86.86 0 0 1 .261.641v3a.872.872 0 0 1-.257.64.872.872 0 0 1-.641.258h-3a.872.872 0 0 1-.898-.895c0-.253.085-.464.257-.64a.86.86 0 0 1 .641-.261ZM5.398 5.398V7.5a.872.872 0 0 1-.895.898.878.878 0 0 1-.64-.257.86.86 0 0 1-.261-.641v-3c0-.254.086-.469.257-.64a.872.872 0 0 1 .641-.258h3a.872.872 0 0 1 .898.895.878.878 0 0 1-.257.64.86.86 0 0 1-.641.261Zm13.204 0H16.5a.872.872 0 0 1-.898-.895c0-.253.085-.464.257-.64a.86.86 0 0 1 .641-.261h3c.254 0 .469.086.64.257a.872.872 0 0 1 .258.641v3a.872.872 0 0 1-.895.898.878.878 0 0 1-.64-.257.86.86 0 0 1-.261-.641Zm0 0"></path>
      </g>
      <g id="group-filled">
        <path d="M2.398 16.898a2.335 2.335 0 0 1 1.176-2.046 11.97 11.97 0 0 1 2.864-1.227 11.566 11.566 0 0 1 3.164-.426c1.097 0 2.152.14 3.16.426 1.008.285 1.965.691 2.863 1.227a2.335 2.335 0 0 1 1.176 2.047v.5c0 .5-.176.925-.528 1.277a1.736 1.736 0 0 1-1.273.523H4.2c-.5 0-.927-.176-1.274-.523a1.74 1.74 0 0 1-.528-1.278ZM18.125 19.2c.148-.281.266-.574.352-.875.082-.3.125-.61.125-.926v-.5c0-.699-.165-1.347-.489-1.949a4.215 4.215 0 0 0-1.312-1.472c.648.132 1.277.312 1.887.535.609.226 1.187.504 1.738.84a2.335 2.335 0 0 1 1.176 2.047v.5c0 .5-.176.925-.528 1.277a1.736 1.736 0 0 1-1.273.523ZM7.051 10.95C6.35 10.25 6 9.4 6 8.4c0-1 .352-1.848 1.05-2.547.7-.704 1.552-1.051 2.552-1.051s1.847.347 2.546 1.05c.704.7 1.051 1.547 1.051 2.547 0 1-.347 1.852-1.05 2.551-.7.7-1.547 1.051-2.547 1.051-1 0-1.852-.352-2.551-1.05Zm9.898 0C16.25 11.65 15.4 12 14.4 12c-.133 0-.258-.004-.376-.012a2.497 2.497 0 0 1-.375-.062 5.859 5.859 0 0 0 .989-1.614A5 5 0 0 0 15 8.399a4.98 4.98 0 0 0-.363-1.91 5.802 5.802 0 0 0-.989-1.613c.137-.035.262-.055.375-.063.118-.007.243-.011.375-.011 1 0 1.852.347 2.551 1.05.7.7 1.051 1.547 1.051 2.547 0 1-.352 1.852-1.05 2.551Zm0 0"></path>
      </g>
      <g id="help">
        <path d="M12.852 17.648c.23-.23.347-.515.347-.847 0-.336-.117-.617-.347-.852a1.158 1.158 0 0 0-.852-.347c-.332 0-.617.113-.852.347-.23.235-.347.516-.347.852 0 .332.117.617.347.847.235.235.52.352.852.352.332 0 .617-.117.852-.352ZM12 21.602a9.369 9.369 0 0 1-3.727-.75 9.706 9.706 0 0 1-3.062-2.063 9.706 9.706 0 0 1-2.063-3.062A9.369 9.369 0 0 1 2.398 12c0-1.332.25-2.578.75-3.738A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063c1.168-.5 2.41-.75 3.727-.75 1.332 0 2.578.25 3.738.75a9.743 9.743 0 0 1 3.051 2.063 9.743 9.743 0 0 1 2.063 3.05c.5 1.16.75 2.407.75 3.739 0 1.316-.25 2.559-.75 3.727a9.706 9.706 0 0 1-2.063 3.062 9.743 9.743 0 0 1-3.05 2.063c-1.16.5-2.407.75-3.739.75Zm0-1.801c2.168 0 4.008-.758 5.523-2.278 1.52-1.515 2.278-3.355 2.278-5.523 0-2.168-.758-4.008-2.278-5.523-1.515-1.52-3.355-2.278-5.523-2.278-2.168 0-4.008.758-5.523 2.278C4.957 7.992 4.199 9.832 4.199 12c0 2.168.758 4.008 2.278 5.523 1.515 1.52 3.355 2.278 5.523 2.278ZM12 12Zm.074-4.3c.434 0 .805.132 1.114.402.308.265.46.605.46 1.023 0 .367-.105.691-.324.977a4.67 4.67 0 0 1-.75.773A6.528 6.528 0 0 0 11.551 12c-.301.418-.442.883-.426 1.398 0 .235.082.422.25.563a.888.888 0 0 0 .602.215c.23 0 .433-.07.597-.215a.954.954 0 0 0 .324-.563 2.34 2.34 0 0 1 .477-.972c.234-.285.484-.551.75-.801.383-.367.695-.773.938-1.227a3.01 3.01 0 0 0 .363-1.449c0-.847-.328-1.554-.989-2.113C13.777 6.277 13 6 12.102 6a3.96 3.96 0 0 0-1.79.414 3.196 3.196 0 0 0-1.335 1.211.767.767 0 0 0-.114.648.71.71 0 0 0 .41.5.948.948 0 0 0 .704.079 1.14 1.14 0 0 0 .574-.375c.183-.235.406-.422.676-.563.265-.144.546-.215.847-.215Zm0 0"></path>
      </g>
      <g id="history">
        <path d="M12 20.398c-2.2 0-4.086-.726-5.664-2.187-1.574-1.457-2.469-3.254-2.688-5.387a.628.628 0 0 1 .215-.586A.908.908 0 0 1 4.5 12c.234 0 .441.066.625.2a.779.779 0 0 1 .324.55c.184 1.648.899 3.04 2.137 4.164C8.828 18.04 10.3 18.602 12 18.602c1.816 0 3.371-.649 4.664-1.938 1.29-1.293 1.938-2.848 1.938-4.664 0-1.816-.649-3.371-1.938-4.664-1.293-1.29-2.848-1.938-4.664-1.938a6.453 6.453 0 0 0-2.863.641 6.372 6.372 0 0 0-2.211 1.762h1.773c.254 0 .469.086.64.258a.852.852 0 0 1 .263.636.86.86 0 0 1-.262.64.864.864 0 0 1-.64.267H4.5a.86.86 0 0 1-.64-.262.872.872 0 0 1-.258-.64V4.5a.872.872 0 0 1 .895-.898c.253 0 .464.086.64.257a.86.86 0 0 1 .261.641v2.352c.77-1 1.723-1.793 2.864-2.375 1.14-.586 2.386-.875 3.738-.875 1.168 0 2.258.218 3.277.664a8.502 8.502 0 0 1 2.66 1.796 8.502 8.502 0 0 1 1.797 2.66c.446 1.02.664 2.11.664 3.278a8.086 8.086 0 0 1-.664 3.277 8.502 8.502 0 0 1-1.797 2.66 8.502 8.502 0 0 1-2.66 1.797 8.086 8.086 0 0 1-3.277.664Zm.898-9.148 2.25 2.25c.184.184.274.39.266.625a.924.924 0 0 1-.289.625.87.87 0 0 1-.637.273.87.87 0 0 1-.636-.273l-2.47-2.469a.97.97 0 0 1-.206-.297.83.83 0 0 1-.074-.347V8.094c0-.254.085-.465.261-.637A.875.875 0 0 1 12 7.199c.25 0 .46.086.637.258a.869.869 0 0 1 .261.645Zm0 0"></path>
      </g>
      <g id="info-filled">
        <path d="M12.637 16.543a.869.869 0 0 0 .261-.645V11.7a.872.872 0 0 0-.895-.898.878.878 0 0 0-.64.258.86.86 0 0 0-.261.64v4.2a.872.872 0 0 0 .895.902.878.878 0 0 0 .64-.258Zm0-7.8a.852.852 0 0 0 .261-.638.9.9 0 0 0-.253-.644.875.875 0 0 0-.641-.262.878.878 0 0 0-.64.258.852.852 0 0 0-.262.637.9.9 0 0 0 .253.644.86.86 0 0 0 .641.262.878.878 0 0 0 .64-.258Zm-.63 12.859c-1.32 0-2.566-.25-3.734-.75a9.706 9.706 0 0 1-3.062-2.063 9.706 9.706 0 0 1-2.063-3.062 9.398 9.398 0 0 1-.75-3.739c0-1.324.25-2.566.75-3.726A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063 9.398 9.398 0 0 1 3.739-.75c1.324 0 2.566.25 3.726.75a9.743 9.743 0 0 1 3.051 2.063 9.688 9.688 0 0 1 2.063 3.059c.5 1.16.75 2.402.75 3.722 0 1.32-.25 2.567-.75 3.735a9.706 9.706 0 0 1-2.063 3.062 9.688 9.688 0 0 1-3.059 2.063 9.31 9.31 0 0 1-3.722.75Zm0 0"></path>
      </g>
      <g id="info">
        <path d="M12.637 16.543a.869.869 0 0 0 .261-.645V11.7a.872.872 0 0 0-.895-.898.878.878 0 0 0-.64.258.86.86 0 0 0-.261.64v4.2a.872.872 0 0 0 .895.902.878.878 0 0 0 .64-.258Zm0-7.8a.852.852 0 0 0 .261-.638.9.9 0 0 0-.253-.644.875.875 0 0 0-.641-.262.878.878 0 0 0-.64.258.852.852 0 0 0-.262.637.9.9 0 0 0 .253.644.86.86 0 0 0 .641.262.878.878 0 0 0 .64-.258Zm-.63 12.859c-1.32 0-2.566-.25-3.734-.75a9.706 9.706 0 0 1-3.062-2.063 9.706 9.706 0 0 1-2.063-3.062 9.398 9.398 0 0 1-.75-3.739c0-1.324.25-2.566.75-3.726A9.743 9.743 0 0 1 5.211 5.21a9.706 9.706 0 0 1 3.062-2.063 9.398 9.398 0 0 1 3.739-.75c1.324 0 2.566.25 3.726.75a9.743 9.743 0 0 1 3.051 2.063 9.688 9.688 0 0 1 2.063 3.059c.5 1.16.75 2.402.75 3.722 0 1.32-.25 2.567-.75 3.735a9.706 9.706 0 0 1-2.063 3.062 9.688 9.688 0 0 1-3.059 2.063 9.31 9.31 0 0 1-3.722.75ZM12 19.8c2.168 0 4.008-.758 5.523-2.278 1.52-1.515 2.278-3.355 2.278-5.523 0-2.168-.758-4.008-2.278-5.523-1.515-1.52-3.355-2.278-5.523-2.278-2.168 0-4.008.758-5.523 2.278C4.957 7.992 4.199 9.832 4.199 12c0 2.168.758 4.008 2.278 5.523 1.515 1.52 3.355 2.278 5.523 2.278ZM12 12Zm0 0"></path>
      </g>
      <g id="draft-filled">
        <path d="M6.594 21.602c-.496 0-.918-.18-1.27-.532a1.74 1.74 0 0 1-.523-1.27V4.2c0-.497.176-.919.527-1.27a1.74 1.74 0 0 1 1.274-.532h7.046c.235 0 .465.043.688.125.226.086.422.22.59.403l3.75 3.75c.183.164.316.363.398.586.086.226.125.453.125.687v11.852c0 .496-.176.918-.527 1.27a1.747 1.747 0 0 1-1.274.53ZM13.199 7.5a.88.88 0 0 0 .902.898h3.297L13.2 4.2Zm0 0"></path>
      </g>
      <g id="location-on-filled">
        <path d="M12.012 12.898c.742 0 1.375-.261 1.902-.785a2.629 2.629 0 0 0 .785-1.925c0-.743-.261-1.376-.785-1.899A2.603 2.603 0 0 0 12 7.5c-.75 0-1.387.262-1.914.79A2.6 2.6 0 0 0 9.3 10.2c0 .75.261 1.386.785 1.913a2.629 2.629 0 0 0 1.926.785ZM12 21.625a1.07 1.07 0 0 1-.504-.125.69.69 0 0 1-.328-.375l-.172-.352a9.497 9.497 0 0 0-.594-.953 17.9 17.9 0 0 0-.675-.894 26.445 26.445 0 0 0-2.23-2.113A10.273 10.273 0 0 1 5.5 14.55a7.304 7.304 0 0 1-.977-2.055 8.07 8.07 0 0 1-.324-2.27c0-2.164.75-4.007 2.254-5.535 1.5-1.527 3.336-2.293 5.512-2.293 2.172 0 4.023.766 5.547 2.29C19.039 6.21 19.8 8.063 19.8 10.233c0 .778-.117 1.532-.344 2.266a8.121 8.121 0 0 1-.98 2.05 11.185 11.185 0 0 1-2 2.266 25.79 25.79 0 0 0-2.176 2.11 9.348 9.348 0 0 0-.739.887 6.452 6.452 0 0 0-.585.96l-.176.352c-.086.148-.2.27-.344.363a.854.854 0 0 1-.457.137Zm0 0"></path>
      </g>
      <g id="mic-filled">
        <path d="M9.875 13.523A2.892 2.892 0 0 1 9 11.398v-6c0-.832.293-1.539.875-2.125A2.898 2.898 0 0 1 12 2.398c.832 0 1.543.293 2.125.875.582.586.875 1.293.875 2.125v6c0 .836-.293 1.543-.875 2.125-.582.586-1.293.875-2.125.875-.832 0-1.543-.289-2.125-.875Zm1.227 5.977v-1.574a6.285 6.285 0 0 1-3.766-1.84 6.399 6.399 0 0 1-1.86-3.762.768.768 0 0 1 .196-.652.817.817 0 0 1 .625-.274.87.87 0 0 1 .64.266c.176.172.29.387.336.637.22 1.148.774 2.086 1.665 2.812a4.722 4.722 0 0 0 3.05 1.086 4.716 4.716 0 0 0 3.075-1.097 4.676 4.676 0 0 0 1.664-2.801c.046-.25.16-.465.332-.637a.855.855 0 0 1 .636-.266c.254 0 .465.094.63.274.167.18.234.398.198.652a6.399 6.399 0 0 1-1.859 3.762 6.285 6.285 0 0 1-3.766 1.84V19.5a.872.872 0 0 1-.895.898.878.878 0 0 1-.64-.257.86.86 0 0 1-.261-.641Zm0 0"></path>
      </g>
      <g id="more-vert">
        <path d="M11.996 19.2c-.496 0-.922-.177-1.27-.532a1.737 1.737 0 0 1-.527-1.273c0-.497.176-.918.531-1.27a1.734 1.734 0 0 1 1.274-.523c.496 0 .922.175 1.27.527.351.355.527.781.527 1.277 0 .496-.176.918-.531 1.27a1.749 1.749 0 0 1-1.274.523Zm0-5.4c-.496 0-.922-.175-1.27-.53a1.737 1.737 0 0 1-.527-1.274c0-.496.176-.922.531-1.27a1.737 1.737 0 0 1 1.274-.527c.496 0 .922.176 1.27.531.351.352.527.778.527 1.274s-.176.922-.531 1.27a1.737 1.737 0 0 1-1.274.527Zm0-5.402c-.496 0-.922-.175-1.27-.527a1.752 1.752 0 0 1-.527-1.277c0-.496.176-.918.531-1.27a1.749 1.749 0 0 1 1.274-.523c.496 0 .922.176 1.27.531.351.352.527.777.527 1.273 0 .497-.176.918-.531 1.27a1.734 1.734 0 0 1-1.274.523Zm0 0"></path>
      </g>
      <g id="open-in-new">
        <path d="M5.398 20.398c-.492 0-.918-.175-1.27-.527a1.735 1.735 0 0 1-.526-1.27V5.399c0-.492.175-.918.527-1.27a1.735 1.735 0 0 1 1.27-.526h5.703a.872.872 0 0 1 .898.895.878.878 0 0 1-.258.64.86.86 0 0 1-.64.261H5.398v13.204h13.204v-5.704a.872.872 0 0 1 .895-.898c.253 0 .464.086.64.258a.86.86 0 0 1 .261.64v5.704c0 .492-.175.918-.527 1.27a1.735 1.735 0 0 1-1.27.526ZM18.602 6.676l-8.301 8.3a.816.816 0 0 1-.625.262.907.907 0 0 1-.625-.289.874.874 0 0 1-.278-.636c0-.243.094-.454.278-.637l8.273-8.278h-2.023a.884.884 0 0 1-.903-.894c0-.254.086-.465.262-.64a.86.86 0 0 1 .64-.262h4.2c.254 0 .469.086.64.257a.872.872 0 0 1 .258.641v4.2a.884.884 0 0 1-.894.902.866.866 0 0 1-.64-.262.86.86 0 0 1-.262-.64Zm0 0"></path>
      </g>
      <g id="person">
        <path
          d="M9.45 10.95c-.7-.7-1.052-1.552-1.052-2.552S8.75 6.551 9.45 5.852C10.15 5.148 11 4.8 12 4.8c1 0 1.852.347 2.55 1.05.7.7 1.052 1.547 1.052 2.547 0 1-.352 1.852-1.051 2.551C13.85 11.65 13 12 12 12c-1 0-1.852-.352-2.55-1.05ZM4.8 17.397v-.597c0-.383.102-.746.313-1.09a2.63 2.63 0 0 1 .864-.86 12.207 12.207 0 0 1 2.906-1.226 11.602 11.602 0 0 1 6.23 0c1.024.285 1.996.691 2.91 1.227.368.214.657.5.864.847.21.352.312.719.312 1.102v.597c0 .497-.176.922-.527 1.274a1.744 1.744 0 0 1-1.274.527H6.594c-.496 0-.918-.176-1.27-.527a1.749 1.749 0 0 1-.523-1.274Zm1.802 0h10.796v-.597a.39.39 0 0 0-.074-.235.49.49 0 0 0-.199-.168 9.306 9.306 0 0 0-2.45-1.046A10.203 10.203 0 0 0 12 15c-.918 0-1.809.117-2.676.352a9.306 9.306 0 0 0-2.449 1.046 1.357 1.357 0 0 0-.2.196.314.314 0 0 0-.073.207Zm6.671-7.73c.352-.352.528-.777.528-1.273 0-.497-.176-.918-.531-1.27a1.734 1.734 0 0 1-1.274-.523c-.496 0-.922.175-1.27.527a1.752 1.752 0 0 0-.527 1.277c0 .496.176.918.531 1.27.352.347.778.523 1.274.523s.922-.176 1.27-.531ZM12 8.398Zm0 9Zm0 0"
        </path>
      </g>
      <g id="person-filled">
        <path d="M9.45 10.95c-.7-.7-1.052-1.552-1.052-2.552S8.75 6.551 9.45 5.852C10.15 5.148 11 4.8 12 4.8c1 0 1.852.347 2.55 1.05.7.7 1.052 1.547 1.052 2.547 0 1-.352 1.852-1.051 2.551C13.85 11.65 13 12 12 12c-1 0-1.852-.352-2.55-1.05ZM4.8 17.397v-.597c0-.383.102-.746.313-1.09a2.63 2.63 0 0 1 .864-.86 12.235 12.235 0 0 1 2.91-1.226 11.573 11.573 0 0 1 6.226 0c1.024.285 1.996.691 2.91 1.227.368.214.657.5.864.847.21.352.312.719.312 1.102v.597c0 .5-.176.926-.523 1.278a1.751 1.751 0 0 1-1.278.523H6.602c-.5 0-.926-.176-1.278-.523a1.751 1.751 0 0 1-.523-1.278Zm0 0"></path>
      </g>
      <g id="print-filled">
        <path d="M7.8 20.398c-.495 0-.917-.175-1.273-.527A1.729 1.729 0 0 1 6 18.601v-1.796H4.2c-.497 0-.919-.176-1.27-.528A1.743 1.743 0 0 1 2.398 15v-4.2c0-.667.235-1.234.704-1.698A2.303 2.303 0 0 1 4.8 8.398h14.398c.668 0 1.235.235 1.7.704.468.464.703 1.03.703 1.699V15c0 .496-.176.918-.532 1.27a1.715 1.715 0 0 1-1.265.53h-1.801v1.802c0 .492-.176.918-.527 1.27a1.74 1.74 0 0 1-1.278.526ZM18 7.2H6V5.395c0-.497.176-.918.527-1.27a1.74 1.74 0 0 1 1.274-.523h8.398c.496 0 .918.175 1.274.527.351.351.527.777.527 1.27Zm-.305 5.403a.872.872 0 0 0 .64-.258.861.861 0 0 0 .267-.637.887.887 0 0 0-.258-.645.858.858 0 0 0-.637-.261.887.887 0 0 0-.645.258.852.852 0 0 0-.261.636c0 .254.086.47.258.64a.855.855 0 0 0 .636.267Zm-9.894 6h8.398V15H7.801Zm0 0"></path>
      </g>
      <g id="schedule">
        <path d="M12.898 11.25V6.898A.872.872 0 0 0 12.003 6a.878.878 0 0 0-.64.258.86.86 0 0 0-.261.64v4.727c0 .133.023.254.074.363.05.11.117.203.199.285l3.348 3.348a.828.828 0 0 0 .64.266.914.914 0 0 0 .637-.285.879.879 0 0 0 .273-.641.885.885 0 0 0-.28-.64ZM12 21.602a9.347 9.347 0 0 1-3.73-.75 9.688 9.688 0 0 1-3.06-2.063 9.706 9.706 0 0 1-2.062-3.062 9.398 9.398 0 0 1-.75-3.739c0-1.324.25-2.57.75-3.738A9.524 9.524 0 0 1 5.211 5.2a9.805 9.805 0 0 1 3.062-2.052 9.398 9.398 0 0 1 3.739-.75c1.324 0 2.57.254 3.738.758a9.689 9.689 0 0 1 3.047 2.051 9.762 9.762 0 0 1 2.047 3.05A9.342 9.342 0 0 1 21.602 12c0 1.324-.25 2.566-.75 3.73a9.788 9.788 0 0 1-2.051 3.06 9.541 9.541 0 0 1-3.055 2.062c-1.168.5-2.418.75-3.746.75ZM12 12Zm.012 7.8c2.156 0 3.996-.76 5.511-2.288 1.52-1.524 2.278-3.367 2.278-5.524 0-2.156-.758-3.996-2.278-5.511-1.515-1.52-3.355-2.278-5.511-2.278-2.157 0-4 .758-5.524 2.278C4.961 7.992 4.2 9.832 4.2 11.988c0 2.157.762 4 2.29 5.524 1.523 1.527 3.366 2.289 5.523 2.289Zm0 0"></path>
      </g>
      <g id="search">
        <path d="M9.602 15.602c-1.668 0-3.086-.586-4.25-1.75-1.168-1.168-1.75-2.586-1.75-4.25 0-1.668.582-3.086 1.75-4.25 1.164-1.168 2.582-1.75 4.25-1.75 1.664 0 3.082.582 4.25 1.75 1.164 1.164 1.75 2.582 1.75 4.25a5.75 5.75 0 0 1-.313 1.902 6.293 6.293 0 0 1-.863 1.644l5.347 5.352c.184.184.278.39.278.625a.862.862 0 0 1-.278.625.87.87 0 0 1-.636.273.87.87 0 0 1-.637-.273l-5.352-5.324c-.5.367-1.046.652-1.644.863a5.75 5.75 0 0 1-1.902.313Zm0-1.801c1.164 0 2.156-.41 2.972-1.227.817-.816 1.227-1.808 1.227-2.972 0-1.168-.41-2.16-1.227-2.977-.816-.816-1.808-1.227-2.972-1.227-1.168 0-2.16.41-2.977 1.227-.816.816-1.227 1.809-1.227 2.977 0 1.164.41 2.156 1.227 2.972.816.817 1.809 1.227 2.977 1.227Zm0 0"></path>
      </g>
      <g id="security">
        <path d="M12 19.727c1.617-.5 2.96-1.465 4.04-2.891A9.466 9.466 0 0 0 17.925 12H12V4.324L6 6.625v4.5c0 .148.008.297.023.438.02.14.036.289.051.437H12Zm-.3 1.761a1 1 0 0 1-.274-.062c-2.184-.719-3.934-2.043-5.25-3.977-1.317-1.933-1.977-4.039-1.977-6.324v-4.5a1.763 1.763 0 0 1 1.152-1.676l6-2.3A1.82 1.82 0 0 1 12 2.522c.215 0 .434.043.648.125l6 2.301a1.763 1.763 0 0 1 1.153 1.676v4.5c0 2.285-.66 4.39-1.977 6.324-1.316 1.934-3.066 3.258-5.25 3.977a1 1 0 0 1-.273.062 3.87 3.87 0 0 1-.602 0Zm0 0"></path>
      </g>
      <!-- The <g> IDs are exposed as global variables in Vulcanized mode, which
        conflicts with the "settings" namespace of MD Settings. Using an "_icon"
        suffix prevents the naming conflict. -->
      <g id="settings-filled">
        <path d="M11 21.602c-.285 0-.523-.086-.727-.25a1.097 1.097 0 0 1-.375-.653l-.375-1.949a8.72 8.72 0 0 1-1.109-.523c-.36-.204-.7-.434-1.016-.704l-1.875.653c-.25.082-.503.078-.761-.012a1.066 1.066 0 0 1-.586-.488l-1-1.75a1.045 1.045 0 0 1-.125-.739 1.2 1.2 0 0 1 .375-.664l1.472-1.296a8.15 8.15 0 0 1-.074-.602 8.05 8.05 0 0 1 .074-1.852L3.426 9.477a1.2 1.2 0 0 1-.375-.665 1.045 1.045 0 0 1 .125-.738l1-1.75c.133-.234.328-.394.586-.488.258-.09.511-.094.761-.012l1.875.653c.317-.27.657-.5 1.016-.704a8.72 8.72 0 0 1 1.11-.523l.374-1.95c.051-.265.176-.484.375-.652.204-.164.442-.25.727-.25h2c.285 0 .523.086.727.25.199.168.324.387.375.653l.375 1.949a8.72 8.72 0 0 1 1.109.523c.36.204.7.434 1.016.704l1.875-.653c.25-.082.503-.078.761.012.258.094.453.254.586.488l1 1.75c.133.235.176.48.125.739a1.2 1.2 0 0 1-.375.664l-1.472 1.296c.03.204.058.403.074.602a8.05 8.05 0 0 1-.074 1.852l1.472 1.296c.2.184.324.407.375.665.051.257.008.503-.125.738l-1 1.75a1.066 1.066 0 0 1-.586.488c-.258.09-.511.094-.761.012l-1.875-.653c-.317.27-.657.5-1.016.704a8.72 8.72 0 0 1-1.11.523l-.374 1.95c-.051.265-.176.484-.375.652-.204.164-.442.25-.727.25Zm1-6c1 0 1.852-.352 2.55-1.051.7-.7 1.052-1.551 1.052-2.551 0-1-.352-1.852-1.051-2.55C13.85 8.75 13 8.397 12 8.397c-1 0-1.852.352-2.55 1.051-.7.7-1.052 1.551-1.052 2.551 0 1 .352 1.852 1.051 2.55.7.7 1.551 1.052 2.551 1.052Zm0 0"></path>
      </g>
      <g id="star-filled">
        <path d="m12 16.875-4.102 2.45a.892.892 0 0 1-1-.05.876.876 0 0 1-.296-.412.849.849 0 0 1-.028-.539l1.074-4.574-3.625-3.074a.834.834 0 0 1-.273-.465 1.08 1.08 0 0 1 .023-.512.83.83 0 0 1 .278-.41c.133-.11.3-.172.5-.187l4.75-.426 1.875-4.352c.082-.183.199-.32.347-.41a.89.89 0 0 1 .477-.14.89.89 0 0 1 .477.14c.148.09.265.227.347.41L14.7 8.7l4.75.403c.2.015.367.082.5.199a.91.91 0 0 1 .278.426c.046.164.05.332.011.5a.892.892 0 0 1-.289.449l-3.597 3.074 1.074 4.574c.05.184.039.363-.028.54a.876.876 0 0 1-.761.585.915.915 0 0 1-.535-.125Zm0 0"></path>
      </g>
      <g id="sync">
        <path d="M6.602 12c0 .8.16 1.543.484 2.227a5.47 5.47 0 0 0 1.312 1.75V15.3c0-.254.086-.469.258-.64a.858.858 0 0 1 .637-.263c.254 0 .469.086.645.262a.86.86 0 0 1 .261.64v3a.872.872 0 0 1-.258.641.872.872 0 0 1-.64.258h-3a.872.872 0 0 1-.64-.258.852.852 0 0 1-.263-.636.86.86 0 0 1 .262-.64.864.864 0 0 1 .64-.267h.95a7.418 7.418 0 0 1-1.79-2.386A6.906 6.906 0 0 1 4.8 12c0-1.418.368-2.695 1.098-3.836A7.072 7.072 0 0 1 8.813 5.57a.659.659 0 0 1 .636-.011.862.862 0 0 1 .45.52.9.9 0 0 1-.012.683.98.98 0 0 1-.45.511 5.425 5.425 0 0 0-2.066 1.942c-.516.824-.77 1.75-.77 2.785Zm10.796 0c0-.8-.16-1.543-.484-2.227a5.47 5.47 0 0 0-1.312-1.75V8.7a.872.872 0 0 1-.258.64.858.858 0 0 1-.637.263.875.875 0 0 1-.645-.262.86.86 0 0 1-.261-.64v-3c0-.255.086-.47.258-.641a.872.872 0 0 1 .64-.258h3c.254 0 .469.086.64.258a.852.852 0 0 1 .263.636.86.86 0 0 1-.262.64.864.864 0 0 1-.64.267h-.95a7.418 7.418 0 0 1 1.79 2.386c.44.926.66 1.93.66 3.012 0 1.418-.368 2.695-1.098 3.836a7.072 7.072 0 0 1-2.915 2.594.649.649 0 0 1-.636.008.87.87 0 0 1-.45-.524.911.911 0 0 1 .422-1.164 5.458 5.458 0 0 0 2.079-1.926c.53-.832.796-1.773.796-2.824Zm0 0"></path>
      </g>
      <g id="thumb-down">
        <path d="M3 15.602c-.45 0-.863-.188-1.238-.563-.375-.375-.563-.789-.563-1.238v-1.426c0-.133.012-.258.04-.375.023-.117.062-.227.113-.324l2.972-6.977c.153-.347.43-.625.84-.824.406-.2.8-.3 1.188-.3l11.046.027v12l-5.847 5.847c-.317.317-.68.524-1.09.613-.406.09-.774.047-1.098-.136-.324-.184-.535-.48-.625-.887-.093-.41-.09-.855.012-1.34l.852-4.097Zm12.602-.75V5.398H6l-3 6.977v1.426h8.8l-1.226 6.05ZM21 3.602c.516 0 .945.175 1.29.527.339.351.51.777.51 1.27V13.8c0 .496-.171.918-.51 1.27a1.732 1.732 0 0 1-1.29.53h-3.602v-1.8H21V5.398h-3.602V3.602Zm-5.398 1.796v9.454Zm0 0"></path>
      </g>
      <g id="thumb-down-filled">
        <path d="M3 15.602c-.45 0-.863-.184-1.238-.551-.375-.367-.563-.785-.563-1.25v-1.426c0-.133.012-.254.04-.363.023-.11.062-.219.113-.336l2.972-6.977a1.72 1.72 0 0 1 .653-.8c.296-.2.64-.297 1.023-.297h9.602c.5 0 .921.168 1.273.511.352.34.523.77.523 1.285v9.454c0 .25-.046.484-.148.71a1.943 1.943 0 0 1-.398.586l-5.028 5.028c-.3.3-.664.492-1.097.574-.434.082-.801.035-1.102-.148a1.496 1.496 0 0 1-.738-.954 2.596 2.596 0 0 1-.035-1.222l.75-3.824Zm18-12c.484 0 .902.175 1.262.535.36.36.539.781.539 1.261v8.403c0 .484-.18.902-.54 1.261-.359.36-.777.54-1.261.54-.484 0-.902-.18-1.262-.54-.36-.359-.539-.777-.539-1.261V5.398c0-.48.18-.902.54-1.261A1.72 1.72 0 0 1 21 3.602Zm0 0"></path>
      </g>
      <g id="thumb-up">
        <path d="M21 8.375c.45 0 .863.188 1.238.563.375.374.563.789.563 1.238v1.426c0 .132-.012.257-.04.375a1.33 1.33 0 0 1-.113.324l-2.972 6.972c-.153.352-.43.625-.84.829-.406.199-.8.296-1.188.296l-11.046-.023v-12l5.847-5.852a2.18 2.18 0 0 1 1.09-.609c.406-.094.774-.047 1.098.137.324.183.535.476.625.886.093.41.09.856-.012 1.336l-.852 4.102Zm-12.602.75v9.45H18l3-6.973v-1.426h-8.8l1.226-6.051ZM3 20.375c-.516 0-.945-.176-1.29-.527a1.776 1.776 0 0 1-.51-1.274v-8.398c0-.496.171-.918.51-1.274.345-.351.774-.527 1.29-.527h3.602v1.8H3v8.4h3.602v1.8Zm5.398-1.8v-9.45Zm0 0"></path>
      </g>
      <g id="thumb-up-filled">
        <path d="M21 8.398c.45 0 .863.184 1.238.551.375.367.563.785.563 1.25v1.426c0 .133-.012.254-.04.363-.023.11-.062.219-.113.336l-2.972 6.977a1.72 1.72 0 0 1-.653.8c-.296.2-.64.297-1.023.297H8.398c-.5 0-.921-.168-1.273-.511-.352-.34-.523-.77-.523-1.285V9.148c0-.25.046-.484.148-.71.102-.227.234-.422.398-.586l5.028-5.028c.3-.3.664-.492 1.097-.574.434-.082.801-.035 1.102.148.383.22.629.536.738.954.11.414.121.824.035 1.222l-.75 3.824Zm-18 12a1.72 1.72 0 0 1-1.262-.535c-.36-.36-.539-.781-.539-1.261v-8.403c0-.484.18-.902.54-1.261.359-.36.777-.54 1.261-.54.484 0 .902.18 1.262.54.36.359.539.777.539 1.261v8.403c0 .48-.18.902-.54 1.261A1.72 1.72 0 0 1 3 20.398Zm0 0"></path>
      </g>
      <g id="videocam">
        <path d="M5.398 19.2c-.48 0-.902-.18-1.261-.536a1.729 1.729 0 0 1-.535-1.266V6.602c0-.497.175-.922.535-1.274a1.75 1.75 0 0 1 1.261-.527H16.2c.496 0 .918.176 1.274.527.351.352.527.777.527 1.274V10.8l2.824-2.824c.133-.137.297-.168.488-.098.192.07.29.203.29.394v7.45c0 .203-.098.336-.29.402-.19.066-.355.035-.488-.102L18 13.2v4.2c0 .484-.176.906-.527 1.265a1.736 1.736 0 0 1-1.274.535Zm0-1.802H16.2V6.602H5.4Zm0 0V6.602Zm0 0"></path>
      </g>
      <g id="warning-filled">
        <path d="M2.797 20.398a.91.91 0 0 1-.477-.12.86.86 0 0 1-.32-.329.765.765 0 0 1-.113-.449c.008-.168.054-.324.136-.477l9.204-15.347a.727.727 0 0 1 .335-.324c.145-.067.29-.102.438-.102.148 0 .297.035.438.102.14.066.253.171.335.324l9.204 15.347c.082.153.128.309.136.477a.765.765 0 0 1-.113.45.99.99 0 0 1-.324.323.855.855 0 0 1-.473.125Zm9.84-3.253a.86.86 0 0 0 .261-.641.89.89 0 0 0-.253-.64.86.86 0 0 0-.641-.262.89.89 0 0 0-.64.253.86.86 0 0 0-.262.641.89.89 0 0 0 .253.64.86.86 0 0 0 .641.262.89.89 0 0 0 .64-.253Zm0-3.004a.86.86 0 0 0 .261-.641v-3a.872.872 0 0 0-.895-.898.878.878 0 0 0-.64.257.86.86 0 0 0-.261.641v3a.872.872 0 0 0 .895.898.878.878 0 0 0 .64-.257Zm0 0"></path>
      </g>
    </defs>
  </svg>
</cr-iconset>`;
} else {
  div.innerHTML = getTrustedHTML`
<cr-iconset name="cr20" size="20">
  <svg>
    <defs>
      <!--
      Keep these in sorted order by id="".
      -->
      <g id="block">
        <path fill-rule="evenodd" clip-rule="evenodd"
          d="M10 0C4.48 0 0 4.48 0 10C0 15.52 4.48 20 10 20C15.52 20 20 15.52 20 10C20 4.48 15.52 0 10 0ZM2 10C2 5.58 5.58 2 10 2C11.85 2 13.55 2.63 14.9 3.69L3.69 14.9C2.63 13.55 2 11.85 2 10ZM5.1 16.31C6.45 17.37 8.15 18 10 18C14.42 18 18 14.42 18 10C18 8.15 17.37 6.45 16.31 5.1L5.1 16.31Z">
        </path>
      </g>
      <g id="cloud-off">
        <path
          d="M16 18.125L13.875 16H5C3.88889 16 2.94444 15.6111 2.16667 14.8333C1.38889 14.0556 1 13.1111 1 12C1 10.9444 1.36111 10.0347 2.08333 9.27083C2.80556 8.50694 3.6875 8.09028 4.72917 8.02083C4.77083 7.86805 4.8125 7.72222 4.85417 7.58333C4.90972 7.44444 4.97222 7.30555 5.04167 7.16667L1.875 4L2.9375 2.9375L17.0625 17.0625L16 18.125ZM5 14.5H12.375L6.20833 8.33333C6.15278 8.51389 6.09722 8.70139 6.04167 8.89583C6 9.07639 5.95139 9.25694 5.89583 9.4375L4.83333 9.52083C4.16667 9.57639 3.61111 9.84028 3.16667 10.3125C2.72222 10.7708 2.5 11.3333 2.5 12C2.5 12.6944 2.74306 13.2847 3.22917 13.7708C3.71528 14.2569 4.30556 14.5 5 14.5ZM17.5 15.375L16.3958 14.2917C16.7153 14.125 16.9792 13.8819 17.1875 13.5625C17.3958 13.2431 17.5 12.8889 17.5 12.5C17.5 11.9444 17.3056 11.4722 16.9167 11.0833C16.5278 10.6944 16.0556 10.5 15.5 10.5H14.125L14 9.14583C13.9028 8.11806 13.4722 7.25694 12.7083 6.5625C11.9444 5.85417 11.0417 5.5 10 5.5C9.65278 5.5 9.31944 5.54167 9 5.625C8.69444 5.70833 8.39583 5.82639 8.10417 5.97917L7.02083 4.89583C7.46528 4.61806 7.93056 4.40278 8.41667 4.25C8.91667 4.08333 9.44444 4 10 4C11.4306 4 12.6736 4.48611 13.7292 5.45833C14.7847 6.41667 15.375 7.59722 15.5 9C16.4722 9 17.2986 9.34028 17.9792 10.0208C18.6597 10.7014 19 11.5278 19 12.5C19 13.0972 18.8611 13.6458 18.5833 14.1458C18.3194 14.6458 17.9583 15.0556 17.5 15.375Z">
        </path>
      </g>
      <g id="delete">
        <path
          d="M 5.832031 17.5 C 5.375 17.5 4.984375 17.335938 4.65625 17.011719 C 4.328125 16.683594 4.167969 16.292969 4.167969 15.832031 L 4.167969 5 L 3.332031 5 L 3.332031 3.332031 L 7.5 3.332031 L 7.5 2.5 L 12.5 2.5 L 12.5 3.332031 L 16.667969 3.332031 L 16.667969 5 L 15.832031 5 L 15.832031 15.832031 C 15.832031 16.292969 15.671875 16.683594 15.34375 17.011719 C 15.015625 17.335938 14.625 17.5 14.167969 17.5 Z M 14.167969 5 L 5.832031 5 L 5.832031 15.832031 L 14.167969 15.832031 Z M 7.5 14.167969 L 9.167969 14.167969 L 9.167969 6.667969 L 7.5 6.667969 Z M 10.832031 14.167969 L 12.5 14.167969 L 12.5 6.667969 L 10.832031 6.667969 Z M 5.832031 5 L 5.832031 15.832031 Z M 5.832031 5 ">
        </path>
      </g>
      <g id="domain" viewBox="0 -960 960 960">
        <path d="M96-144v-672h384v144h384v528H96Zm72-72h72v-72h-72v72Zm0-152h72v-72h-72v72Zm0-152h72v-72h-72v72Zm0-152h72v-72h-72v72Zm168 456h72v-72h-72v72Zm0-152h72v-72h-72v72Zm0-152h72v-72h-72v72Zm0-152h72v-72h-72v72Zm144 456h312v-384H480v80h72v72h-72v80h72v72h-72v80Zm168-232v-72h72v72h-72Zm0 152v-72h72v72h-72Z"></path>
      </g>
      <g id="family-link">
        <path fill-rule="evenodd" clip-rule="evenodd"
          d="M4.6327 8.00094L10.3199 2L16 8.00094L10.1848 16.8673C10.0995 16.9873 10.0071 17.1074 9.90047 17.2199C9.42417 17.7225 8.79147 18 8.11611 18C7.44076 18 6.80806 17.7225 6.33175 17.2199C5.85545 16.7173 5.59242 16.0497 5.59242 15.3371C5.59242 14.977 5.46445 14.647 5.22275 14.3919C4.98104 14.1369 4.66825 14.0019 4.32701 14.0019H4V12.6667H4.32701C5.00237 12.6667 5.63507 12.9442 6.11137 13.4468C6.58768 13.9494 6.85071 14.617 6.85071 15.3296C6.85071 15.6896 6.97867 16.0197 7.22038 16.2747C7.46209 16.5298 7.77488 16.6648 8.11611 16.6648C8.45735 16.6648 8.77014 16.5223 9.01185 16.2747C9.02396 16.2601 9.03607 16.246 9.04808 16.2319C9.08541 16.1883 9.12176 16.1458 9.15403 16.0947L9.55213 15.4946L4.6327 8.00094ZM10.3199 13.9371L6.53802 8.17116L10.3199 4.1814L14.0963 8.17103L10.3199 13.9371Z">
        </path>
      </g>
      <g id="menu">
        <path d="M2 4h16v2H2zM2 9h16v2H2zM2 14h16v2H2z"></path>
      </g>
      <g id="password-manager">
        <path d="M5.833 11.667c.458 0 .847-.16 1.167-.479.333-.333.5-.729.5-1.188s-.167-.847-.5-1.167a1.555 1.555 0 0 0-1.167-.5c-.458 0-.854.167-1.188.5A1.588 1.588 0 0 0 4.166 10c0 .458.16.854.479 1.188.333.319.729.479 1.188.479Zm0 3.333c-1.389 0-2.569-.486-3.542-1.458C1.319 12.569.833 11.389.833 10c0-1.389.486-2.569 1.458-3.542C3.264 5.486 4.444 5 5.833 5c.944 0 1.813.243 2.604.729a4.752 4.752 0 0 1 1.833 1.979h7.23c.458 0 .847.167 1.167.5.333.319.5.708.5 1.167v3.958c0 .458-.167.854-.5 1.188A1.588 1.588 0 0 1 17.5 15h-3.75a1.658 1.658 0 0 1-1.188-.479 1.658 1.658 0 0 1-.479-1.188v-1.042H10.27a4.59 4.59 0 0 1-1.813 2A5.1 5.1 0 0 1 5.833 15Zm3.292-4.375h4.625v2.708H15v-1.042a.592.592 0 0 1 .167-.438.623.623 0 0 1 .458-.188c.181 0 .327.063.438.188a.558.558 0 0 1 .188.438v1.042H17.5V9.375H9.125a3.312 3.312 0 0 0-1.167-1.938 3.203 3.203 0 0 0-2.125-.77 3.21 3.21 0 0 0-2.354.979C2.827 8.298 2.5 9.083 2.5 10s.327 1.702.979 2.354a3.21 3.21 0 0 0 2.354.979c.806 0 1.514-.25 2.125-.75.611-.514 1-1.167 1.167-1.958Z"></path>
      </g>
      
  </svg>
</cr-iconset>

<!-- NOTE: In the common case that the final icon will be 20x20, export the SVG
     at 20px and place it in the section above. -->
<cr-iconset name="cr" size="24">
  <svg>
    <defs>
      <!--
      These icons are copied from Polymer's iron-icons and kept in sorted order.
      -->
      <g id="add">
        <path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z" />
      </g>
      <g id="arrow-back">
        <path
          d="m7.824 13 5.602 5.602L12 20l-8-8 8-8 1.426 1.398L7.824 11H20v2Zm0 0">
        </path>
      </g>
      <g id="arrow-drop-up">
        <path d="M7 14l5-5 5 5z"></path>
      </g>
      <g id="arrow-drop-down">
        <path d="M7 10l5 5 5-5z"></path>
      </g>
      <g id="arrow-forward">
        <path
          d="M12 4l-1.41 1.41L16.17 11H4v2h12.17l-5.58 5.59L12 20l8-8z">
        </path>
      </g>
      <g id="arrow-right">
        <path d="M10 7l5 5-5 5z"></path>
      </g>
      <g id="cancel-filled">
        <path
          d="M12 2C6.47 2 2 6.47 2 12s4.47 10 10 10 10-4.47 10-10S17.53 2 12 2zm5 13.59L15.59 17 12 13.41 8.41 17 7 15.59 10.59 12 7 8.41 8.41 7 12 10.59 15.59 7 17 8.41 13.41 12 17 15.59z">
        </path>
      </g>
      <g id="check">
        <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"></path>
      </g>
      <g id="check-circle" viewBox="0 -960 960 960">
        <path d="m424-296 282-282-56-56-226 226-114-114-56 56 170 170Zm56 216q-83 0-156-31.5T197-197q-54-54-85.5-127T80-480q0-83 31.5-156T197-763q54-54 127-85.5T480-880q83 0 156 31.5T763-763q54 54 85.5 127T880-480q0 83-31.5 156T763-197q-54 54-127 85.5T480-80Zm0-80q134 0 227-93t93-227q0-134-93-227t-227-93q-134 0-227 93t-93 227q0 134 93 227t227 93Zm0-320Z"></path>
      </g>
      <g id="chevron-left">
        <path d="M15.41 7.41L14 6l-6 6 6 6 1.41-1.41L10.83 12z"></path>
      </g>
      <g id="chevron-right">
        <path d="M10 6L8.59 7.41 13.17 12l-4.58 4.59L10 18l6-6z"></path>
      </g>
      <g id="chrome-product" viewBox="0 -960 960 960">
        <path d="M336-479q0 60 42 102t102 42q60 0 102-42t42-102q0-60-42-102t-102-42q-60 0-102 42t-42 102Zm144 216q11 0 22.5-.5T525-267L427-99q-144-16-237.5-125T96-479q0-43 9.5-84.5T134-645l160 274q28 51 78 79.5T480-263Zm0-432q-71 0-126.5 42T276-545l-98-170q53-71 132.5-109.5T480-863q95 0 179 45t138 123H480Zm356 72q15 35 21.5 71t6.5 73q0 155-100 260.5T509-96l157-275q14-25 22-52t8-56q0-40-15-77t-41-67h196Z">
        </path>
      </g>
      <g id="close">
        <path
          d="M19 6.41L17.59 5 12 10.59 6.41 5 5 6.41 10.59 12 5 17.59 6.41 19 12 13.41 17.59 19 19 17.59 13.41 12z">
        </path>
      </g>
      <g id="computer">
        <path
          d="M20 18c1.1 0 1.99-.9 1.99-2L22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2H0v2h24v-2h-4zM4 6h16v10H4V6z">
        </path>
      </g>
      <g id="edit-filled">
        <path
          d="M3 17.25V21h3.75L17.81 9.94l-3.75-3.75L3 17.25zM20.71 7.04c.39-.39.39-1.02 0-1.41l-2.34-2.34c-.39-.39-1.02-.39-1.41 0l-1.83 1.83 3.75 3.75 1.83-1.83z">
        </path>
      </g>
      <g id="delete" viewBox="0 -960 960 960">
        <path
          d="M309.37-135.87q-34.48 0-58.74-24.26-24.26-24.26-24.26-58.74v-474.5h-53.5v-83H378.5v-53.5h202.52v53.5h206.11v83h-53.5v474.07q0 35.21-24.26 59.32t-58.74 24.11H309.37Zm341.26-557.5H309.37v474.5h341.26v-474.5ZM379.7-288.24h77.5v-336h-77.5v336Zm123.1 0h77.5v-336h-77.5v336ZM309.37-693.37v474.5-474.5Z">
        </path>
      </g>
      <g id="devices">
        <path
          d="M4 6h18V4H4c-1.1 0-2 .9-2 2v11H0v3h14v-3H4V6zm19 2h-6c-.55 0-1 .45-1 1v10c0 .55.45 1 1 1h6c.55 0 1-.45 1-1V9c0-.55-.45-1-1-1zm-1 9h-4v-7h4v7z">
        </path>
      </g>
      <g id="domain">
        <path
          d="M12 7V3H2v18h20V7H12zM6 19H4v-2h2v2zm0-4H4v-2h2v2zm0-4H4V9h2v2zm0-4H4V5h2v2zm4 12H8v-2h2v2zm0-4H8v-2h2v2zm0-4H8V9h2v2zm0-4H8V5h2v2zm10 12h-8v-2h2v-2h-2v-2h2v-2h-2V9h8v10zm-2-8h-2v2h2v-2zm0 4h-2v2h2v-2z">
        </path>
      </g>
      <g id="download" viewBox="0 -960 960 960">
        <path d="M480-336 288-528l51-51 105 105v-342h72v342l105-105 51 51-192 192ZM263.72-192Q234-192 213-213.15T192-264v-72h72v72h432v-72h72v72q0 29.7-21.16 50.85Q725.68-192 695.96-192H263.72Z"></path>
      </g>
      <g id="draft-filled">
        <path
          d="M6 2c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6H6zm7 7V3.5L18.5 9H13z">
        </path>
      </g>
      <!-- source: https://fonts.google.com/icons?selected=Material+Symbols+Outlined:family_link:FILL@0;wght@0;GRAD@0;opsz@24&icon.size=24&icon.color=%23e8eaed -->
      <g id="family-link" viewBox="0 -960 960 960">
        <path
          d="M390-40q-51 0-90.5-30.5T246-149q-6-23-25-37t-43-14q-16 0-30 6.5T124-175l-61-51q21-26 51.5-40t63.5-14q51 0 91 30t54 79q6 23 25 37t42 14q19 0 34-10t26-25l1-2-276-381q-8-11-11.5-23t-3.5-24q0-16 6-30.5t18-26.5l260-255q11-11 26-17t30-6q15 0 30 6t26 17l260 255q12 12 18 26.5t6 30.5q0 12-3.5 24T825-538L500-88q-18 25-48 36.5T390-40Zm110-185 260-360-260-255-259 256 259 359Zm1-308Z"/>
        </path>
      </g>
      <g id="error-filled">
        <path
          d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-2h2v2zm0-4h-2V7h2v6z">
        </path>
      </g>
      <g id="error">
        <path
          d="M11 15h2v2h-2zm0-8h2v6h-2zm.99-5C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8z">
        </path>
      </g>
      <g id="keyboard-arrow-up">
        <path d="M12 8l-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14z"></path>
      </g>
      <g id="keyboard-arrow-down">
        <path d="M16.59 8.59L12 13.17 7.41 8.59 6 10l6 6 6-6z"></path>
      </g>
      <g id="chrome-extension-filled">
        <path
          d="M20.5 11H19V7c0-1.1-.9-2-2-2h-4V3.5C13 2.12 11.88 1 10.5 1S8 2.12 8 3.5V5H4c-1.1 0-1.99.9-1.99 2v3.8H3.5c1.49 0 2.7 1.21 2.7 2.7s-1.21 2.7-2.7 2.7H2V20c0 1.1.9 2 2 2h3.8v-1.5c0-1.49 1.21-2.7 2.7-2.7 1.49 0 2.7 1.21 2.7 2.7V22H17c1.1 0 2-.9 2-2v-4h1.5c1.38 0 2.5-1.12 2.5-2.5S21.88 11 20.5 11z">
        </path>
      </g>
      <g id="fullscreen">
        <path
          d="M7 14H5v5h5v-2H7v-3zm-2-4h2V7h3V5H5v5zm12 7h-3v2h5v-5h-2v3zM14 5v2h3v3h2V5h-5z">
        </path>
      </g>
      <g id="group-filled">
        <path
          d="M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5c-1.66 0-3 1.34-3 3s1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5C6.34 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z">
        </path>
      </g>
      <g id="help">
        <path
          d="M11 18h2v-2h-2v2zm1-16C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm0-14c-2.21 0-4 1.79-4 4h2c0-1.1.9-2 2-2s2 .9 2 2c0 2-3 1.75-3 5h2c0-2.25 3-2.5 3-5 0-2.21-1.79-4-4-4z">
        </path>
      </g>
      <g id="history">
        <path
          d="M12.945312 22.75 C 10.320312 22.75 8.074219 21.839844 6.207031 20.019531 C 4.335938 18.199219 3.359375 15.972656 3.269531 13.34375 L 5.089844 13.34375 C 5.175781 15.472656 5.972656 17.273438 7.480469 18.742188 C 8.988281 20.210938 10.808594 20.945312 12.945312 20.945312 C 15.179688 20.945312 17.070312 20.164062 18.621094 18.601562 C 20.167969 17.039062 20.945312 15.144531 20.945312 12.910156 C 20.945312 10.714844 20.164062 8.855469 18.601562 7.335938 C 17.039062 5.816406 15.15625 5.054688 12.945312 5.054688 C 11.710938 5.054688 10.554688 5.339844 9.480469 5.902344 C 8.402344 6.46875 7.476562 7.226562 6.699219 8.179688 L 9.585938 8.179688 L 9.585938 9.984375 L 3.648438 9.984375 L 3.648438 4.0625 L 5.453125 4.0625 L 5.453125 6.824219 C 6.386719 5.707031 7.503906 4.828125 8.804688 4.199219 C 10.109375 3.566406 11.488281 3.25 12.945312 3.25 C 14.300781 3.25 15.570312 3.503906 16.761719 4.011719 C 17.949219 4.519531 18.988281 5.214844 19.875 6.089844 C 20.761719 6.964844 21.464844 7.992188 21.976562 9.167969 C 22.492188 10.34375 22.75 11.609375 22.75 12.964844 C 22.75 14.316406 22.492188 15.589844 21.976562 16.777344 C 21.464844 17.964844 20.761719 19.003906 19.875 19.882812 C 18.988281 20.765625 17.949219 21.464844 16.761719 21.976562 C 15.570312 22.492188 14.300781 22.75 12.945312 22.75 Z M 16.269531 17.460938 L 12.117188 13.34375 L 12.117188 7.527344 L 13.921875 7.527344 L 13.921875 12.601562 L 17.550781 16.179688 Z M 16.269531 17.460938">
        </path>
      </g>
      <g id="info-filled">
        <path
          d="M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z">
        </path>
      </g>
      <g id="info">
        <path
          d="M11 17h2v-6h-2v6zm1-15C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zM11 9h2V7h-2v2z">
        </path>
      </g>
      <g id="insert-drive-file">
        <path
          d="M6 2c-1.1 0-1.99.9-1.99 2L4 20c0 1.1.89 2 1.99 2H18c1.1 0 2-.9 2-2V8l-6-6H6zm7 7V3.5L18.5 9H13z">
        </path>
      </g>
      <g id="location-on-filled">
        <path
          d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z">
        </path>
      </g>
      <g id="mic-filled">
        <path
          d="M12 14c1.66 0 2.99-1.34 2.99-3L15 5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3zm5.3-3c0 3-2.54 5.1-5.3 5.1S6.7 14 6.7 11H5c0 3.41 2.72 6.23 6 6.72V21h2v-3.28c3.28-.48 6-3.3 6-6.72h-1.7z">
        </path>
      </g>
      <g id="more-vert">
        <path
          d="M12 8c1.1 0 2-.9 2-2s-.9-2-2-2-2 .9-2 2 .9 2 2 2zm0 2c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2zm0 6c-1.1 0-2 .9-2 2s.9 2 2 2 2-.9 2-2-.9-2-2-2z">
        </path>
      </g>
      <g id="open-in-new" viewBox="0 -960 960 960">
        <path
          d="M216-144q-29.7 0-50.85-21.15Q144-186.3 144-216v-528q0-29.7 21.15-50.85Q186.3-816 216-816h264v72H216v528h528v-264h72v264q0 29.7-21.15 50.85Q773.7-144 744-144H216Zm171-192-51-51 357-357H576v-72h240v240h-72v-117L387-336Z">
        </path>
      </g>
      <g id="person">
        <path
          d="M12 5.9c1.16 0 2.1.94 2.1 2.1s-.94 2.1-2.1 2.1S9.9 9.16 9.9 8s.94-2.1 2.1-2.1m0 9c2.97 0 6.1 1.46 6.1 2.1v1.1H5.9V17c0-.64 3.13-2.1 6.1-2.1M12 4C9.79 4 8 5.79 8 8s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm0 9c-2.67 0-8 1.34-8 4v3h16v-3c0-2.66-5.33-4-8-4z">
        </path>
      </g>
      <g id="person-filled">
        <path
          d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z">
        </path>
      </g>
      <g id="print-filled">
        <path
          d="M19 8H5c-1.66 0-3 1.34-3 3v6h4v4h12v-4h4v-6c0-1.66-1.34-3-3-3zm-3 11H8v-5h8v5zm3-7c-.55 0-1-.45-1-1s.45-1 1-1 1 .45 1 1-.45 1-1 1zm-1-9H6v4h12V3z">
        </path>
      </g>
      <g id="schedule">
        <path
          d="M11.99 2C6.47 2 2 6.48 2 12s4.47 10 9.99 10C17.52 22 22 17.52 22 12S17.52 2 11.99 2zM12 20c-4.42 0-8-3.58-8-8s3.58-8 8-8 8 3.58 8 8-3.58 8-8 8zm.5-13H11v6l5.25 3.15.75-1.23-4.5-2.67z">
        </path>
      </g>
      <g id="search">
        <path
          d="M15.5 14h-.79l-.28-.27C15.41 12.59 16 11.11 16 9.5 16 5.91 13.09 3 9.5 3S3 5.91 3 9.5 5.91 16 9.5 16c1.61 0 3.09-.59 4.23-1.57l.27.28v.79l5 4.99L20.49 19l-4.99-5zm-6 0C7.01 14 5 11.99 5 9.5S7.01 5 9.5 5 14 7.01 14 9.5 11.99 14 9.5 14z">
        </path>
      </g>
      <g id="security">
        <path
          d="M12 1L3 5v6c0 5.55 3.84 10.74 9 12 5.16-1.26 9-6.45 9-12V5l-9-4zm0 10.99h7c-.53 4.12-3.28 7.79-7 8.94V12H5V6.3l7-3.11v8.8z">
        </path>
      </g>
      <!-- The <g> IDs are exposed as global variables in Vulcanized mode, which
        conflicts with the "settings" namespace of MD Settings. Using an "_icon"
        suffix prevents the naming conflict. -->
      <g id="settings-filled">
        <path
          d="M19.43 12.98c.04-.32.07-.64.07-.98s-.03-.66-.07-.98l2.11-1.65c.19-.15.24-.42.12-.64l-2-3.46c-.12-.22-.39-.3-.61-.22l-2.49 1c-.52-.4-1.08-.73-1.69-.98l-.38-2.65C14.46 2.18 14.25 2 14 2h-4c-.25 0-.46.18-.49.42l-.38 2.65c-.61.25-1.17.59-1.69.98l-2.49-1c-.23-.09-.49 0-.61.22l-2 3.46c-.13.22-.07.49.12.64l2.11 1.65c-.04.32-.07.65-.07.98s.03.66.07.98l-2.11 1.65c-.19.15-.24.42-.12.64l2 3.46c.12.22.39.3.61.22l2.49-1c.52.4 1.08.73 1.69.98l.38 2.65c.03.24.24.42.49.42h4c.25 0 .46-.18.49-.42l.38-2.65c.61-.25 1.17-.59 1.69-.98l2.49 1c.23.09.49 0 .61-.22l2-3.46c.12-.22.07-.49-.12-.64l-2.11-1.65zM12 15.5c-1.93 0-3.5-1.57-3.5-3.5s1.57-3.5 3.5-3.5 3.5 1.57 3.5 3.5-1.57 3.5-3.5 3.5z">
        </path>
      </g>
      <g id="star-filled">
        <path
          d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z">
        </path>
      </g>
      <g id="sync" viewBox="0 -960 960 960">
        <path
          d="M216-192v-72h74q-45-40-71.5-95.5T192-480q0-101 61-177.5T408-758v75q-63 23-103.5 77.5T264-480q0 48 19.5 89t52.5 70v-63h72v192H216Zm336-10v-75q63-23 103.5-77.5T696-480q0-48-19.5-89T624-639v63h-72v-192h192v72h-74q45 40 71.5 95.5T768-480q0 101-61 177.5T552-202Z">
        </path>
      </g>
      <g id="thumb-down">
        <path
            d="M6 3h11v13l-7 7-1.25-1.25a1.454 1.454 0 0 1-.3-.475c-.067-.2-.1-.392-.1-.575v-.35L9.45 16H3c-.533 0-1-.2-1.4-.6-.4-.4-.6-.867-.6-1.4v-2c0-.117.017-.242.05-.375s.067-.258.1-.375l3-7.05c.15-.333.4-.617.75-.85C5.25 3.117 5.617 3 6 3Zm9 2H6l-3 7v2h9l-1.35 5.5L15 15.15V5Zm0 10.15V5v10.15Zm2 .85v-2h3V5h-3V3h5v13h-5Z">
        </path>
      </g>
      <g id="thumb-down-filled">
        <path
            d="M6 3h10v13l-7 7-1.25-1.25a1.336 1.336 0 0 1-.29-.477 1.66 1.66 0 0 1-.108-.574v-.347L8.449 16H3c-.535 0-1-.2-1.398-.602C1.199 15 1 14.535 1 14v-2c0-.117.012-.242.04-.375.022-.133.062-.258.108-.375l3-7.05c.153-.333.403-.618.75-.848A1.957 1.957 0 0 1 6 3Zm12 13V3h4v13Zm0 0">
        </path>
      </g>
      <g id="thumb-up">
        <path
            d="M18 21H7V8l7-7 1.25 1.25c.117.117.208.275.275.475.083.2.125.392.125.575v.35L14.55 8H21c.533 0 1 .2 1.4.6.4.4.6.867.6 1.4v2c0 .117-.017.242-.05.375s-.067.258-.1.375l-3 7.05c-.15.333-.4.617-.75.85-.35.233-.717.35-1.1.35Zm-9-2h9l3-7v-2h-9l1.35-5.5L9 8.85V19ZM9 8.85V19 8.85ZM7 8v2H4v9h3v2H2V8h5Z">
        </path>
      </g>
      <g id="thumb-up-filled">
        <path
            d="M18 21H8V8l7-7 1.25 1.25c.117.117.21.273.29.477.073.199.108.39.108.574v.347L15.551 8H21c.535 0 1 .2 1.398.602C22.801 9 23 9.465 23 10v2c0 .117-.012.242-.04.375a1.897 1.897 0 0 1-.108.375l-3 7.05a2.037 2.037 0 0 1-.75.848A1.957 1.957 0 0 1 18 21ZM6 8v13H2V8Zm0 0">
      </g>
      <g id="videocam" viewBox="0 -960 960 960">
        <path
          d="M216-192q-29 0-50.5-21.5T144-264v-432q0-29.7 21.5-50.85Q187-768 216-768h432q29.7 0 50.85 21.15Q720-725.7 720-696v168l144-144v384L720-432v168q0 29-21.15 50.5T648-192H216Zm0-72h432v-432H216v432Zm0 0v-432 432Z">
        </path>
      </g>
      <g id="warning-filled">
        <path d="M1 21h22L12 2 1 21zm12-3h-2v-2h2v2zm0-4h-2v-4h2v4z"></path>
      </g>
    </defs>
  </svg>
</cr-iconset>`;
}
var iconsets = div.querySelectorAll("cr-iconset");
for (const iconset of iconsets) {
  document.head.appendChild(iconset);
}
function getCss16() {
  return [getCss3(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_hidden_style_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  display: block;\n  position: absolute;\n  outline: none;\n  z-index: 1002;\n  user-select: none;\n  cursor: default;\n}\n\n#tooltip {\n  display: block;\n  outline: none;\n  font-size: 10px;\n  line-height: 1;\n  background-color: var(--paper-tooltip-background, #616161);\n  color: var(--paper-tooltip-text-color, white);\n  padding: 8px;\n  border-radius: 2px;\n}\n\n@keyframes keyFrameFadeInOpacity {\n  0% {\n    opacity: 0;\n  }\n  100% {\n    opacity: var(--paper-tooltip-opacity, 0.9);\n  }\n}\n\n@keyframes keyFrameFadeOutOpacity {\n  0% {\n    opacity: var(--paper-tooltip-opacity, 0.9);\n  }\n  100% {\n    opacity: 0;\n  }\n}\n\n.fade-in-animation {\n  opacity: 0;\n  animation-delay: var(--paper-tooltip-delay-in, 500ms);\n  animation-name: keyFrameFadeInOpacity;\n  animation-iteration-count: 1;\n  animation-timing-function: ease-in;\n  animation-duration: var(--paper-tooltip-duration-in, 500ms);\n  animation-fill-mode: forwards;\n}\n\n.fade-out-animation {\n  opacity: var(--paper-tooltip-opacity, 0.9);\n  animation-delay: var(--paper-tooltip-delay-out, 0ms);\n  animation-name: keyFrameFadeOutOpacity;\n  animation-iteration-count: 1;\n  animation-timing-function: ease-in;\n  animation-duration: var(--paper-tooltip-duration-out, 500ms);\n  animation-fill-mode: forwards;\n}\n\n/* Fills in the space created by the offset so that users can move their\n * pointer towards the tooltip without having the tooltip disappear. */\n#tooltipOffsetFiller {\n  position: absolute;\n\n  :host([position='top']) & {\n    top: 100%;\n  }\n\n  :host([position='bottom']) & {\n    bottom: 100%;\n  }\n\n  :host([position='left']) & {\n    left: 100%;\n  }\n\n  :host([position='right']) & {\n    right: 100%;\n  }\n\n  :host(:is([position='top'], [position='bottom'])) & {\n    left: 0;\n    height: var(--cr-tooltip-offset);\n    width: 100%;\n  }\n\n  :host(:is([position='left'], [position='right'])) & {\n    top: 0;\n    height: 100%;\n    width: var(--cr-tooltip-offset);\n  }\n}\n"])];
}
function getHtml9() {
  return b2`
<div id="tooltip" hidden part="tooltip">
  <slot></slot>
</div>
<div id="tooltipOffsetFiller"></div>`;
}
var CrTooltipElement = class extends CrLitElement {
  static get is() {
    return "cr-tooltip";
  }
  static get styles() {
    return getCss16();
  }
  render() {
    return getHtml9.bind(this)();
  }
  static get properties() {
    return {
      /**
       * The id of the element that the tooltip is anchored to. This element
       * must be a sibling of the tooltip. If this property is not set,
       * then the tooltip will be centered to the parent node containing it.
       */
      for: { type: String },
      /**
       * Set this to true if you want to manually control when the tooltip
       * is shown or hidden.
       */
      manualMode: { type: Boolean },
      /**
       * Positions the tooltip to the top, right, bottom, left of its content.
       */
      position: { type: String, reflect: true },
      /**
       * If true, no parts of the tooltip will ever be shown offscreen.
       */
      fitToVisibleBounds: { type: Boolean },
      /**
       * The spacing between the top of the tooltip and the element it is
       * anchored to.
       */
      offset: { type: Number },
      /**
       * The delay that will be applied before the `entry` animation is
       * played when showing the tooltip.
       */
      animationDelay: { type: Number },
      /**
       * The delay before the tooltip hides itself after moving the pointer
       * away from the tooltip or target.
       */
      hideDelay: { type: Number }
    };
  }
  #animationDelay = 500;
  get animationDelay() {
    return this.#animationDelay;
  }
  set animationDelay(_2) {
    this.#animationDelay = _2;
  }
  #fitToVisibleBounds = false;
  get fitToVisibleBounds() {
    return this.#fitToVisibleBounds;
  }
  set fitToVisibleBounds(_2) {
    this.#fitToVisibleBounds = _2;
  }
  #hideDelay = 600;
  get hideDelay() {
    return this.#hideDelay;
  }
  set hideDelay(_2) {
    this.#hideDelay = _2;
  }
  #for = "";
  get for() {
    return this.#for;
  }
  set for(_2) {
    this.#for = _2;
  }
  #manualMode = false;
  get manualMode() {
    return this.#manualMode;
  }
  set manualMode(_2) {
    this.#manualMode = _2;
  }
  #offset = 14;
  get offset() {
    return this.#offset;
  }
  set offset(_2) {
    this.#offset = _2;
  }
  #position = "bottom";
  get position() {
    return this.#position;
  }
  set position(_2) {
    this.#position = _2;
  }
  animationPlaying_ = false;
  showing_ = false;
  manualTarget_;
  target_ = null;
  tracker_ = new EventTracker();
  hideTimeout_ = null;
  connectedCallback() {
    super.connectedCallback();
    this.findTarget_();
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    if (!this.manualMode) {
      this.removeListeners_();
    }
    this.resetHideTimeout_();
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("animationDelay")) {
      this.style.setProperty(
        "--paper-tooltip-delay-in",
        `${this.animationDelay}ms`
      );
    }
  }
  firstUpdated(changedProperties) {
    super.firstUpdated(changedProperties);
    this.addEventListener("animationend", () => this.onAnimationEnd_());
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("for")) {
      this.findTarget_();
    }
    if (changedProperties.has("manualMode")) {
      if (this.manualMode) {
        this.removeListeners_();
      } else {
        this.addListeners_();
      }
    }
    if (changedProperties.has("offset")) {
      this.style.setProperty("--cr-tooltip-offset", `${this.offset}px`);
    }
  }
  /**
   * Returns the target element that this tooltip is anchored to. It is
   * either the element given by the `for` attribute, the element manually
   * specified through the `target` attribute, or the immediate parent of
   * the tooltip.
   */
  get target() {
    if (this.manualTarget_) {
      return this.manualTarget_;
    }
    const ownerRoot = this.getRootNode();
    if (this.for) {
      return ownerRoot.querySelector(`#${this.for}`);
    }
    const parentNode = this.parentNode;
    return !!parentNode && parentNode.nodeType === Node.DOCUMENT_FRAGMENT_NODE ? ownerRoot.host : parentNode;
  }
  /**
   * Sets the target element that this tooltip will be anchored to.
   */
  set target(target) {
    this.manualTarget_ = target;
    this.findTarget_();
  }
  /**
   * Shows the tooltip programmatically
   */
  show() {
    this.resetHideTimeout_();
    if (this.showing_) {
      return;
    }
    if (!!this.textContent && this.textContent.trim() === "") {
      const children = this.shadowRoot.querySelector("slot").assignedElements();
      const hasNonEmptyChild = Array.from(children).some(
        (el) => !!el.textContent && el.textContent.trim() !== ""
      );
      if (!hasNonEmptyChild) {
        return;
      }
    }
    this.showing_ = true;
    this.$.tooltip.hidden = false;
    this.$.tooltip.classList.remove("fade-out-animation");
    this.updatePosition();
    this.animationPlaying_ = true;
    this.$.tooltip.classList.add("fade-in-animation");
  }
  /**
   * Hides the tooltip programmatically
   */
  hide() {
    if (!this.showing_) {
      return;
    }
    if (this.animationPlaying_) {
      this.showing_ = false;
      this.$.tooltip.classList.remove(
        "fade-in-animation",
        "fade-out-animation"
      );
      this.$.tooltip.hidden = true;
      return;
    }
    this.$.tooltip.classList.remove("fade-in-animation");
    this.$.tooltip.classList.add("fade-out-animation");
    this.showing_ = false;
    this.animationPlaying_ = true;
  }
  queueHide_() {
    this.resetHideTimeout_();
    this.hideTimeout_ = setTimeout(() => {
      this.hide();
      this.hideTimeout_ = null;
    }, this.hideDelay);
  }
  resetHideTimeout_() {
    if (this.hideTimeout_ !== null) {
      clearTimeout(this.hideTimeout_);
      this.hideTimeout_ = null;
    }
  }
  updatePosition() {
    if (!this.target_) {
      return;
    }
    const offsetParent = this.offsetParent || this.composedOffsetParent_();
    if (!offsetParent) {
      return;
    }
    const offset = this.offset;
    const parentRect = offsetParent.getBoundingClientRect();
    const targetRect = this.target_.getBoundingClientRect();
    const tooltipRect = this.$.tooltip.getBoundingClientRect();
    const horizontalCenterOffset = (targetRect.width - tooltipRect.width) / 2;
    const verticalCenterOffset = (targetRect.height - tooltipRect.height) / 2;
    const targetLeft = targetRect.left - parentRect.left;
    const targetTop = targetRect.top - parentRect.top;
    let tooltipLeft;
    let tooltipTop;
    switch (this.position) {
      case "top":
        tooltipLeft = targetLeft + horizontalCenterOffset;
        tooltipTop = targetTop - tooltipRect.height - offset;
        break;
      case "bottom":
        tooltipLeft = targetLeft + horizontalCenterOffset;
        tooltipTop = targetTop + targetRect.height + offset;
        break;
      case "left":
        tooltipLeft = targetLeft - tooltipRect.width - offset;
        tooltipTop = targetTop + verticalCenterOffset;
        break;
      case "right":
        tooltipLeft = targetLeft + targetRect.width + offset;
        tooltipTop = targetTop + verticalCenterOffset;
        break;
      default:
        assertNotReachedCase(this.position);
    }
    if (this.fitToVisibleBounds) {
      if (parentRect.left + tooltipLeft + tooltipRect.width > window.innerWidth) {
        this.style.right = "0px";
        this.style.left = "auto";
      } else {
        this.style.left = Math.max(0, tooltipLeft) + "px";
        this.style.right = "auto";
      }
      if (parentRect.top + tooltipTop + tooltipRect.height > window.innerHeight) {
        this.style.bottom = parentRect.height - targetTop + offset + "px";
        this.style.top = "auto";
      } else {
        this.style.top = Math.max(-parentRect.top, tooltipTop) + "px";
        this.style.bottom = "auto";
      }
    } else {
      this.style.left = tooltipLeft + "px";
      this.style.top = tooltipTop + "px";
    }
  }
  findTarget_() {
    if (!this.manualMode) {
      this.removeListeners_();
    }
    this.target_ = this.target;
    if (!this.manualMode) {
      this.addListeners_();
    }
  }
  onAnimationEnd_() {
    this.animationPlaying_ = false;
    if (!this.showing_) {
      this.$.tooltip.classList.remove("fade-out-animation");
      this.$.tooltip.hidden = true;
    }
  }
  addListeners_() {
    if (this.target_) {
      this.tracker_.add(this.target_, "pointerenter", () => this.show());
      this.tracker_.add(this.target_, "focus", () => this.show());
      this.tracker_.add(this.target_, "pointerleave", () => this.queueHide_());
      this.tracker_.add(this.target_, "blur", () => this.hide());
      this.tracker_.add(this.target_, "click", () => this.hide());
    }
    this.tracker_.add(
      this.$.tooltip,
      "animationend",
      () => this.onAnimationEnd_()
    );
    this.tracker_.add(this, "pointerenter", () => this.show());
    this.tracker_.add(this, "pointerleave", () => this.queueHide_());
  }
  removeListeners_() {
    this.tracker_.removeAll();
  }
  /**
   * Polyfills the old offsetParent behavior from before the spec was changed:
   * https://github.com/w3c/csswg-drafts/issues/159
   * This is necessary when the tooltip is inside a <slot>, e.g. when it
   * is used inside a cr-dialog. In such cases, the tooltip's offsetParent
   * will be null.
   */
  composedOffsetParent_() {
    if (this.computedStyleMap().get("display").value === "none") {
      return null;
    }
    for (let ancestor = flatTreeParent(this); ancestor !== null; ancestor = flatTreeParent(ancestor)) {
      if (!(ancestor instanceof Element)) {
        continue;
      }
      const style2 = ancestor.computedStyleMap();
      if (style2.get("display").value === "none") {
        return null;
      }
      if (style2.get("display").value === "contents") {
        continue;
      }
      if (style2.get("position").value !== "static") {
        return ancestor;
      }
      if (ancestor.tagName === "BODY") {
        return ancestor;
      }
    }
    return null;
    function flatTreeParent(element) {
      if (element.assignedSlot) {
        return element.assignedSlot;
      }
      if (element.parentNode instanceof ShadowRoot) {
        return element.parentNode.host;
      }
      return element.parentElement;
    }
  }
};
customElements.define(CrTooltipElement.is, CrTooltipElement);
function getCss17() {
  return [getCss10(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_shared_style_lit.css.js\n * #import=../cr_shared_vars.css.js\n * #scheme=relative\n * #include=cr-shared-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  display: flex;  /* Position independently from the line-height. */\n}\n\ncr-icon {\n  --iron-icon-width: var(--cr-icon-size);\n  --iron-icon-height: var(--cr-icon-size);\n  --iron-icon-fill-color:\n      var(--cr-tooltip-icon-fill-color, var(--google-grey-700));\n}\n\n@media (prefers-color-scheme: dark) {\n  cr-icon {\n    --iron-icon-fill-color:\n        var(--cr-tooltip-icon-fill-color, var(--google-grey-500));\n  }\n}\n"])];
}
function getHtml10() {
  return b2`
<cr-icon id="indicator" tabindex="0" aria-label="${this.iconAriaLabel}"
    aria-describedby="tooltip" icon="${this.iconClass}" role="img">
</cr-icon>
<cr-tooltip id="tooltip" for="indicator" position="${this.tooltipPosition}"
    fit-to-visible-bounds part="tooltip">
  <slot name="tooltip-text">${this.tooltipText}</slot>
</cr-tooltip>`;
}
var CrTooltipIconElement = class extends CrLitElement {
  static get is() {
    return "cr-tooltip-icon";
  }
  static get styles() {
    return getCss17();
  }
  render() {
    return getHtml10.bind(this)();
  }
  static get properties() {
    return {
      iconAriaLabel: { type: String },
      iconClass: { type: String },
      tooltipText: { type: String },
      /** Position of tooltip popup related to the icon. */
      tooltipPosition: { type: String }
    };
  }
  #iconAriaLabel = "";
  get iconAriaLabel() {
    return this.#iconAriaLabel;
  }
  set iconAriaLabel(_2) {
    this.#iconAriaLabel = _2;
  }
  #iconClass = "";
  get iconClass() {
    return this.#iconClass;
  }
  set iconClass(_2) {
    this.#iconClass = _2;
  }
  #tooltipText = "";
  get tooltipText() {
    return this.#tooltipText;
  }
  set tooltipText(_2) {
    this.#tooltipText = _2;
  }
  #tooltipPosition = "top";
  get tooltipPosition() {
    return this.#tooltipPosition;
  }
  set tooltipPosition(_2) {
    this.#tooltipPosition = _2;
  }
  getFocusableElement() {
    return this.$.indicator;
  }
};
customElements.define(CrTooltipIconElement.is, CrTooltipIconElement);
function getCss18() {
  return [getCss3(), i(["/* Copyright 2024 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=../cr_hidden_style_lit.css.js\n * #scheme=relative\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n/* Intentionally empty */\n"])];
}
function getHtml11() {
  return b2`
<cr-tooltip-icon ?hidden="${!this.getIndicatorVisible_()}"
    tooltip-text="${this.getIndicatorTooltip_()}"
    icon-class="${this.getIndicatorIcon_()}"
    icon-aria-label="${this.iconAriaLabel}"
    tooltip-position="${this.tooltipPosition}">
</cr-tooltip-icon>`;
}
var CrPolicyIndicatorElement = class extends CrLitElement {
  static get is() {
    return "cr-policy-indicator";
  }
  static get styles() {
    return getCss18();
  }
  render() {
    return getHtml11.bind(this)();
  }
  static get properties() {
    return {
      iconAriaLabel: { type: String },
      /**
       * Which indicator type to show (or NONE).
       */
      indicatorType: { type: String },
      /**
       * The name associated with the policy source. See
       * chrome.settingsPrivate.PrefObject.controlledByName.
       */
      indicatorSourceName: { type: String },
      tooltipPosition: { type: String }
    };
  }
  #iconAriaLabel = "";
  get iconAriaLabel() {
    return this.#iconAriaLabel;
  }
  set iconAriaLabel(_2) {
    this.#iconAriaLabel = _2;
  }
  #indicatorType = "none";
  get indicatorType() {
    return this.#indicatorType;
  }
  set indicatorType(_2) {
    this.#indicatorType = _2;
  }
  #indicatorSourceName = "";
  get indicatorSourceName() {
    return this.#indicatorSourceName;
  }
  set indicatorSourceName(_2) {
    this.#indicatorSourceName = _2;
  }
  #tooltipPosition = "top";
  get tooltipPosition() {
    return this.#tooltipPosition;
  }
  set tooltipPosition(_2) {
    this.#tooltipPosition = _2;
  }
  /**
   * @return True if the indicator should be shown.
   */
  getIndicatorVisible_() {
    return this.indicatorType !== "none";
  }
  /**
   * @return The iron-icon icon name.
   */
  getIndicatorIcon_() {
    switch (this.indicatorType) {
      case "extension":
        return "cr:chrome-extension-filled";
      case "none":
        return "";
      case "primary_user":
        return "cr:group-filled";
      case "owner":
        return "cr:person-filled";
      case "userPolicy":
      case "devicePolicy":
      case "recommended":
        return "cr20:domain";
      case "parent":
      case "childRestriction":
        return "cr20:family-link";
      default:
        assertNotReachedCase(this.indicatorType);
    }
  }
  /**
   * @param indicatorSourceName The name associated with the indicator.
   *     See chrome.settingsPrivate.PrefObject.controlledByName
   * @return The tooltip text for |type|.
   */
  getIndicatorTooltip_() {
    if (!window.CrPolicyStrings) {
      return "";
    }
    const CrPolicyStrings = window.CrPolicyStrings;
    switch (this.indicatorType) {
      case "extension":
        return this.indicatorSourceName.length > 0 ? CrPolicyStrings.controlledSettingExtension.replace(
          "$1",
          this.indicatorSourceName
        ) : CrPolicyStrings.controlledSettingExtensionWithoutName;
      // 
      case "userPolicy":
      case "devicePolicy":
        return CrPolicyStrings.controlledSettingPolicy;
      case "recommended":
        return CrPolicyStrings.controlledSettingRecommendedDiffers;
      case "parent":
        return CrPolicyStrings.controlledSettingParent;
      case "childRestriction":
        return CrPolicyStrings.controlledSettingChildRestriction;
      case "none":
      case "owner":
      case "primary_user":
        return "";
      default:
        assertNotReachedCase(this.indicatorType);
    }
  }
};
customElements.define(CrPolicyIndicatorElement.is, CrPolicyIndicatorElement);
function sanitizeInnerHtmlInternal(rawString, opts) {
  opts = opts || {};
  const html2 = parseHtmlSubset(`<b>${rawString}</b>`, opts.tags, opts.attrs).firstElementChild;
  return html2.innerHTML;
}
var sanitizedPolicy = null;
function sanitizeInnerHtml(rawString, opts) {
  assert(window.trustedTypes);
  if (sanitizedPolicy === null) {
    sanitizedPolicy = window.trustedTypes.createPolicy("sanitize-inner-html", {
      createHTML: sanitizeInnerHtmlInternal,
      createScript: () => assertNotReached(),
      createScriptURL: () => assertNotReached()
    });
  }
  return sanitizedPolicy.createHTML(rawString, opts);
}
var allowAttribute = (_node, _value) => true;
var allowedAttributes = /* @__PURE__ */ new Map([
  [
    "href",
    (node, value) => {
      return node.tagName === "A" && (value.startsWith("chrome://") || value.startsWith("https://") || value === "#");
    }
  ],
  [
    "target",
    (node, value) => {
      return node.tagName === "A" && value === "_blank";
    }
  ]
]);
var allowedOptionalAttributes = /* @__PURE__ */ new Map([
  ["class", allowAttribute],
  ["id", allowAttribute],
  ["is", (_node, value) => value === "action-link" || value === ""],
  ["role", (_node, value) => value === "link"],
  [
    "src",
    (node, value) => {
      return node.tagName === "IMG" && value.startsWith("chrome://");
    }
  ],
  ["tabindex", allowAttribute],
  ["aria-description", allowAttribute],
  ["aria-hidden", allowAttribute],
  ["aria-label", allowAttribute],
  ["aria-labelledby", allowAttribute]
]);
var allowedTags = /* @__PURE__ */ new Set(
  ["A", "B", "I", "BR", "DIV", "EM", "KBD", "P", "PRE", "SPAN", "STRONG"]
);
var allowedOptionalTags = /* @__PURE__ */ new Set(["IMG", "LI", "UL"]);
var unsanitizedPolicy;
function mergeTags(optTags) {
  const clone = new Set(allowedTags);
  optTags.forEach((str) => {
    const tag = str.toUpperCase();
    if (allowedOptionalTags.has(tag)) {
      clone.add(tag);
    }
  });
  return clone;
}
function mergeAttrs(optAttrs) {
  const clone = new Map(allowedAttributes);
  optAttrs.forEach((key) => {
    if (allowedOptionalAttributes.has(key)) {
      clone.set(key, allowedOptionalAttributes.get(key));
    }
  });
  return clone;
}
function walk(n4, f4) {
  f4(n4);
  for (let i7 = 0; i7 < n4.childNodes.length; i7++) {
    walk(n4.childNodes[i7], f4);
  }
}
function assertElement(tags, node) {
  if (!tags.has(node.tagName)) {
    throw Error(node.tagName + " is not supported");
  }
}
function assertAttribute(attrs, attrNode, node) {
  const n4 = attrNode.nodeName;
  const v3 = attrNode.nodeValue || "";
  if (!attrs.has(n4) || !attrs.get(n4)(node, v3)) {
    throw Error(
      node.tagName + "[" + n4 + '="' + v3 + '"] is not supported'
    );
  }
}
function parseHtmlSubset(s5, extraTags, extraAttrs) {
  const tags = extraTags ? mergeTags(extraTags) : allowedTags;
  const attrs = extraAttrs ? mergeAttrs(extraAttrs) : allowedAttributes;
  const doc = document.implementation.createHTMLDocument("");
  const r5 = doc.createRange();
  r5.selectNode(doc.body);
  if (window.trustedTypes) {
    if (!unsanitizedPolicy) {
      unsanitizedPolicy = window.trustedTypes.createPolicy("parse-html-subset", {
        createHTML: (untrustedHTML) => untrustedHTML,
        createScript: () => assertNotReached(),
        createScriptURL: () => assertNotReached()
      });
    }
    s5 = unsanitizedPolicy.createHTML(s5);
  }
  const df = r5.createContextualFragment(s5);
  walk(df, function(node) {
    switch (node.nodeType) {
      case Node.ELEMENT_NODE:
        assertElement(tags, node);
        const nodeAttrs = node.attributes;
        for (let i7 = 0; i7 < nodeAttrs.length; ++i7) {
          assertAttribute(attrs, nodeAttrs[i7], node);
        }
        break;
      case Node.COMMENT_NODE:
      case Node.DOCUMENT_FRAGMENT_NODE:
      case Node.TEXT_NODE:
        break;
      default:
        throw Error("Node type " + node.nodeType + " is not supported");
    }
  });
  return df;
}
var I18nMixinLit = (superClass) => {
  class I18nMixinLit2 extends superClass {
    /**
     * Returns a translated string where $1 to $9 are replaced by the given
     * values.
     * @param id The ID of the string to translate.
     * @param varArgs Values to replace the placeholders $1 to $9 in the
     *     string.
     * @return A translated, substituted string.
     */
    i18nRaw_(id, ...varArgs) {
      return varArgs.length === 0 ? loadTimeData.getString(id) : loadTimeData.getStringF(id, ...varArgs);
    }
    /**
     * Returns a translated string where $1 to $9 are replaced by the given
     * values. Also sanitizes the output to filter out dangerous HTML/JS.
     * Use with Lit bindings that are *not* innerHTML.
     * NOTE: This is not related to $i18n{foo} in HTML, see file overview.
     * @param id The ID of the string to translate.
     * @param varArgs Values to replace the placeholders $1 to $9 in the
     *     string.
     * @return A translated, sanitized, substituted string.
     */
    i18n(id, ...varArgs) {
      const rawString = this.i18nRaw_(id, ...varArgs);
      return parseHtmlSubset(`<b>${rawString}</b>`).firstChild.textContent;
    }
    /**
     * Similar to 'i18n', returns a translated, sanitized, substituted
     * string. It receives the string ID and a dictionary containing the
     * substitutions as well as optional additional allowed tags and
     * attributes. Use with Lit bindings that are innerHTML.
     * @param id The ID of the string to translate.
     */
    i18nAdvanced(id, opts) {
      opts = opts || {};
      const rawString = this.i18nRaw_(id, ...opts.substitutions || []);
      return sanitizeInnerHtml(rawString, opts);
    }
    /**
     * Similar to 'i18n', with an unused |locale| parameter used to trigger
     * updates when the locale changes.
     * @param locale The UI language used.
     * @param id The ID of the string to translate.
     * @param varArgs Values to replace the placeholders $1 to $9 in the
     *     string.
     * @return A translated, sanitized, substituted string.
     */
    i18nDynamic(_locale, id, ...varArgs) {
      return this.i18n(id, ...varArgs);
    }
    /**
     * Similar to 'i18nDynamic', but varArgs values are interpreted as keys
     * in loadTimeData. This allows generation of strings that take other
     * localized strings as parameters.
     * @param locale The UI language used.
     * @param id The ID of the string to translate.
     * @param varArgs Values to replace the placeholders $1 to $9
     *     in the string. Values are interpreted as strings IDs if found in
     * the list of localized strings.
     * @return A translated, sanitized, substituted string.
     */
    i18nRecursive(locale, id, ...varArgs) {
      let args = varArgs;
      if (args.length > 0) {
        args = args.map((str) => {
          return this.i18nExists(str) ? loadTimeData.getString(str) : str;
        });
      }
      return this.i18nDynamic(locale, id, ...args);
    }
    /**
     * Returns true if a translation exists for |id|.
     */
    i18nExists(id) {
      return loadTimeData.valueExists(id);
    }
  }
  return I18nMixinLit2;
};
function skColorToRgba(skColor) {
  const a3 = skColor.value >> 24 & 255;
  const r5 = skColor.value >> 16 & 255;
  const g2 = skColor.value >> 8 & 255;
  const b3 = skColor.value & 255;
  return `rgba(${r5}, ${g2}, ${b3}, ${(a3 / 255).toFixed(2)})`;
}
function getCss19() {
  return [getCss3(), getCss7(), i([`/* Copyright 2024 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=//resources/cr_elements/cr_shared_vars.css.js
 * #import=//resources/cr_elements/cr_hidden_style_lit.css.js
 * #import=//resources/cr_elements/cr_icons_lit.css.js
 * #scheme=relative
 * #include=cr-hidden-style-lit cr-icons-lit
 * #css_wrapper_metadata_end */

:host {
  --cr-tooltip-icon-fill-color: var(--cr-fallback-color-on-surface-subtle);
  --icon-button-color-active: var(--google-grey-700);
  --icon-button-color: var(--google-grey-600);
  --icon-size: var(--most-visited-icon-size, 48px);
  --managed-tile-background-color: var(--cr-fallback-color-neutral-container);
  --tile-background-color: rgb(229, 231, 232);
  --tile-hover-color: rgba(var(--google-grey-900-rgb), .1);
  --tile-size: var(--most-visited-tile-size, 112px);
  --title-height: var(--most-visited-title-height, 32px);
}

@media (prefers-color-scheme: dark) {
  :host {
    --tile-background-color: var(--google-grey-100);
  }
}

:host([is-dark_]) {
  --icon-button-color-active: var(--google-grey-300);
  --icon-button-color: white;
  --tile-hover-color: rgba(255, 255, 255, .1);
}

#container {
  --content-width: calc(
      var(--column-count) * var(--tile-size) +
      max(0, var(--column-count) - 1) * var(--most-visited-gap, 0px)
      /* We add an extra pixel because rounding errors on different zooms can
       * make the width shorter than it should be. */
      + 1px);
  column-gap: var(--most-visited-gap, 0);
  display: flex;
  flex-wrap: wrap;
  height: calc(var(--row-count) * var(--tile-size));
  justify-content: center;
  margin-bottom: 8px;
  opacity: 0;
  overflow: hidden;
  padding: 2px;  /* Padding added so focus rings are not clipped. */
  transition: opacity 300ms ease-in-out;
  width: calc(var(--content-width) + 12px);
}

:host([visible_]) #container {
  opacity: 1;
}

#addShortcutIcon,
#showMoreIcon,
#showLessIcon,
.query-tile-icon {
  -webkit-mask-repeat: no-repeat;
  -webkit-mask-size: 100%;
  height: 24px;
  width: 24px;
}

.tile-icon-container {
  background-color:
      var(--add-shortcut-background-color, var(--tile-background-color));
  margin-inline-start: auto;
  margin-inline-end: auto;
}

#addShortcutIcon {
  -webkit-mask-image: url("/newtab/chromium/images/add_old.svg");
  background-color:
      var(--add-shortcut-foreground-color, var(--google-grey-900));
}

#showMoreIcon {
  -webkit-mask-image: url("/newtab/chromium/cr_components/most_visited/expand_content_old.svg");
  background-color:
      var(--add-shortcut-foreground-color, var(--google-grey-900));
}

#showLessIcon {
  -webkit-mask-image: url("/newtab/chromium/cr_components/most_visited/collapse_content_old.svg");
  background-color:
      var(--add-shortcut-foreground-color, var(--google-grey-900));
}

.query-tile-icon {
  -webkit-mask-image: url("/newtab/chromium/images/icon_search.svg");
  background-color: var(--google-grey-700);
}

@media (forced-colors: active) {
  #addShortcutIcon,
  #showMoreIcon,
  #showLessIcon,
  .query-tile-icon {
    background-color: ButtonText;
  }
}

:host([use-white-tile-icon_]) #addShortcutIcon,
:host([use-white-tile-icon_]) #showMoreIcon,
:host([use-white-tile-icon_]) #showLessIcon {
  background-color: white;
}

:host([use-white-tile-icon_]) .query-tile-icon {
  background-color: var(--google-grey-400);
}

.tile,
#addShortcut,
#showMore,
#showLess {
  -webkit-app-region: no-drag;
  -webkit-tap-highlight-color: transparent;
  align-items: center;
  border-radius: 4px;
  box-sizing: border-box;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  height: var(--tile-size);
  opacity: 1;
  outline: none;
  position: relative;
  text-decoration: none;
  transition-duration: 300ms;
  transition-property: left, top;
  transition-timing-function: ease-in-out;
  user-select: none;
  width: var(--tile-size);
}

.tile a {
  border-radius: 4px;
  display: inline-block;
  height: 100%;
  outline: none;
  position: absolute;
  touch-action: none;
  width: 100%;
}

:host-context(.focus-outline-visible) .tile a:focus,
:host-context(.focus-outline-visible) #addShortcut:focus,
:host-context(.focus-outline-visible) #showMore:focus,
:host-context(.focus-outline-visible) #showLess:focus {
  box-shadow: var(--most-visited-focus-shadow);
}

@media (forced-colors: active) {
  :host-context(.focus-outline-visible) .tile a:focus,
  :host-context(.focus-outline-visible) #addShortcut:focus,
  :host-context(.focus-outline-visible) #showMore:focus,
  :host-context(.focus-outline-visible) #showLess:focus {
    /* Use outline instead of box-shadow (which does not work) in Windows
        HCM. */
    outline: var(--cr-focus-outline-hcm);
  }
}

#addShortcut,
#showMore,
#showLess {
  --cr-hover-background-color: transparent;
  background-color: transparent;
  border: none;
  box-shadow: none;
  justify-content: unset;
  padding: 0;
}

:host(:not([reordering_])) .tile:hover,
:host(:not([reordering_])) #addShortcut:hover,
:host(:not([reordering_])) #showMore:hover,
:host(:not([reordering_])) #showLess:hover,
.force-hover {
  background-color: var(--tile-hover-color);
}

.tile-icon {
  align-items: center;
  background-color: var(--tile-background-color);
  border-radius: 50%;
  display: flex;
  flex-shrink: 0;
  height: var(--icon-size);
  justify-content: center;
  margin-top: var(--most-visited-icon-margin-top, 16px);
  width: var(--icon-size);
}

.tile-icon img {
  height: 24px;
  width: 24px;
}

.managed-tile-icon {
  align-items: center;
  background-color: var(--managed-tile-background-color);
  border-radius: 50%;
  display: flex;
  flex-shrink: 0;
  height: 24px;
  justify-content: center;
  left: var(--most-visited-managed-icon-left, 68px);
  position: absolute;
  top: var(--most-visited-managed-icon-top, 40px);
  width: 24px;
}

cr-policy-indicator {
  --cr-icon-size: 16px;
}

.tile-title {
  align-items: center;
  border-radius: calc(var(--title-height) / 2 + 2px);
  color: var(--most-visited-text-color);
  display: flex;
  height: var(--title-height);
  line-height: calc(var(--title-height) / 2);
  margin-top: 6px;
  padding: 2px 8px;
  max-width: calc(var(--tile-size) - 10px);
}

.tile-title span {
  font-weight: 400;
  overflow: hidden;
  text-align: center;
  text-overflow: ellipsis;
  text-shadow: var(--most-visited-text-shadow);
  white-space: nowrap;
  width: 100%;
}

.tile[query-tile] .tile-title span {
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  white-space: initial;
}

.title-rtl {
  direction: rtl;
}

.title-ltr {
  direction: ltr;
}

:host([hide-title]) #container {
  margin-bottom: 0;
  padding: 0;
  width: var(--content-width);
}

:host([hide-title]) .tile-title {
  display: none;
}

:host([hide-title]) .tile,
:host([hide-title]) #addShortcut,
:host([hide-title]) #showMore,
:host([hide-title]) #showLess {
  justify-content: center;
}

:host([hide-title]) .tile-icon {
  margin: 0;
}

:host([hide-title]) .tile a {
  border-radius: 50%;
  height: var(--icon-size);
  inset: 0;
  margin: auto;
  width: var(--icon-size);
}

.tile.dragging {
  background-color: var(--tile-hover-color);
  transition-property: none;
  z-index: 2;
}

cr-icon-button {
  --cr-icon-button-fill-color: var(--icon-button-color);
  --cr-icon-button-size: 28px;
  --cr-icon-button-transition: none;
  margin: 4px 2px;
  opacity: 0;
  position: absolute;
  right: 0;
  top: 0;
  transition: opacity 100ms ease-in-out;
}

:host-context([dir=rtl]) cr-icon-button {
  left: 0;
  right: unset;
}

:host(:not([reordering_])) .tile:hover cr-icon-button,
.force-hover cr-icon-button {
  opacity: 1;
  transition-delay: 400ms;
}

:host(:not([reordering_])) cr-icon-button:active,
:host-context(.focus-outline-visible):host(:not([reordering_]))
    cr-icon-button:focus,
:host(:not([reordering_])) cr-icon-button:hover {
  --cr-icon-button-fill-color: var(--icon-button-color-active);
  opacity: 1;
  transition-delay: 0s;
}

#dialogContent {
  height: 160px;
}

/* When the policy subtitle is visible, the dialog content needs to be
 * taller to fit it. The extra 36px accounts for the subtitle's content
 * (~20px) and its bottom margin (16px). */
#dialogContent:has(#policySubtitleContainer:not([hidden])) {
  height: 196px;
}

#policySubtitleContainer {
  --iron-icon-height: 20px;
  --iron-icon-width: 20px;
  gap: 10px;
  display: flex;
  margin-bottom: 16px;
}

cr-toast-manager {
  --cr-toast-white-space: normal;
}

:host-context([webui-rounded-icons]) #addShortcutIcon {
  -webkit-mask-image: url("/newtab/chromium/images/add.svg");
}

:host-context([webui-rounded-icons]) #showMoreIcon {
  -webkit-mask-image: url("/newtab/chromium/cr_components/most_visited/expand_content.svg");
}

:host-context([webui-rounded-icons]) #showLessIcon {
  -webkit-mask-image: url("/newtab/chromium/cr_components/most_visited/collapse_content.svg");
}


`])];
}
function getHtml12() {
  return b2`<!--_html_template_start_-->
<div id="container" ?hidden="${!this.visible_}"
    .style="--tile-background-color: ${this.getBackgroundColorStyle_()};
            --column-count: ${this.columnCount_};
            --row-count: ${this.rowCount_};">
  ${this.tiles_.map((item, index) => b2`
    <div class="tile" ?query-tile="${item.isQueryTile}"
        ?hidden="${this.isHidden_(index)}" title="${item.title}"
        @dragstart="${this.onDragstart_}" @touchstart="${this.onTouchstart_}"
        @click="${this.onTileClick_}" @mouseenter="${this.onTileMouseenter_}"
        @mouseleave="${this.onTileMouseleave_}"
        @mousedown="${this.onTileMousedown_}" @keydown="${this.onTileKeydown_}"
        draggable="${!this.nonEditable}" data-index="${index}">
      <a href="${item.url}" aria-label="${item.title}" draggable="false"></a>
      <cr-icon-button id="actionMenuButton" class="icon-more-vert"
          title="${this.getMoreActionText_(item.title)}"
          @click="${this.onTileActionButtonClick_}" tabindex="0" ?hidden="${this.nonEditable || !this.customLinksEnabled_ && !this.isFromEnterpriseShortcut_(item.source)}"
          data-index="${index}">
      </cr-icon-button>
      <cr-icon-button id="removeButton" class="icon-clear"
          title="${this.getRemoveButtonText_(item.title)}"
          @click="${this.onTileRemoveButtonClick_}" tabindex="0" ?hidden="${this.nonEditable || (this.customLinksEnabled_ || this.isFromEnterpriseShortcut_(item.source))}"
          data-index="${index}">
      </cr-icon-button>
      <div class="tile-icon">
        <img src="${this.getFaviconUrl_(item.url)}" draggable="false"
            ?hidden="${item.isQueryTile}" alt="">
        <div class="query-tile-icon" draggable="false"
            ?hidden="${!item.isQueryTile}">
        </div>
        <div class="managed-tile-icon"
            ?hidden="${!this.isFromEnterpriseShortcut_(item.source)}">
          <cr-policy-indicator indicator-type="userPolicy">
          </cr-policy-indicator>
        </div>
      </div>
      <div class="tile-title ${this.getTileTitleDirectionClass_(item)}"
          ?hidden="${this.hideTitle}">
        <span>${item.title}</span>
      </div>
    </div>
  `)}
  <cr-button id="addShortcut" tabindex="0" @click="${this.onAddClick_}"
      ?hidden="${!this.showAdd_}" @keydown="${this.onAddShortcutKeydown_}"
      aria-label="${this.i18n("addLinkTitle")}"
      title="${this.i18n("addLinkTitle")}" noink>
    <div class="tile-icon tile-icon-container">
      <div id="addShortcutIcon" draggable="false"></div>
    </div>
    <div class="tile-title" ?hidden="${this.hideTitle}">
      <span>${this.i18n("addLinkTitle")}</span>
    </div>
  </cr-button>
  <div>
    <cr-button id="showMore" tabindex="0" @click="${this.onShowMoreClick_}"
        ?hidden="${!this.showShowMore_}" @keydown="${this.onShowMoreKeydown_}"
        aria-label="${this.i18n("showMore")}" title="${this.i18n("showMore")}"
        noink>
      <div class="tile-icon tile-icon-container">
        <div id="showMoreIcon" draggable="false"></div>
      </div>
      <div class="tile-title" ?hidden="${this.hideTitle}">
        <span>${this.i18n("showMore")}</span>
      </div>
    </cr-button>
    <cr-button id="showLess" tabindex="0" @click="${this.onShowLessClick_}"
        ?hidden="${!this.showShowLess_}" @keydown="${this.onShowLessKeydown_}"
        aria-label="${this.i18n("showLess")}" title="${this.i18n("showLess")}"
        noink>
      <div class="tile-icon tile-icon-container">
        <div id="showLessIcon" draggable="false"></div>
      </div>
      <div class="tile-title" ?hidden="${this.hideTitle}">
        <span>${this.i18n("showLess")}</span>
      </div>
    </cr-button>
  </div>
  <cr-dialog id="dialog" consume-keydown-event
      @keydown="${this.onDialogKeydown_}" @close="${this.onDialogClose_}">
    <div slot="title">${this.dialogTitle_}</div>
    <div slot="body" id="dialogContent">
      ${this.isFromEnterpriseShortcut_(this.dialogSource_) ? b2`
        <div id="policySubtitleContainer">
          <cr-icon icon="cr:domain"></cr-icon>
          <span class="secondary">
            ${this.i18n("enterpriseShortcutSubtitle")}
          </span>
        </div>
      ` : ""}
      <cr-input id="dialogInputName" label="${this.i18n("nameField")}"
          .value="${this.dialogTileTitle_}"
          ?readonly="${this.dialogIsReadonly_}" spellcheck="false" autofocus
          @value-changed="${this.onDialogTileNameValueChanged_}">
      </cr-input>
      <cr-input id="dialogInputUrl" label="${this.i18n("urlField")}"
          .value="${this.dialogTileUrl_}"
          ?invalid="${this.dialogTileUrlInvalid_}"
          .errorMessage="${this.dialogTileUrlError_}" spellcheck="false"
          type="url" @blur="${this.onDialogTileUrlBlur_}"
          @value-changed="${this.onDialogTileUrlValueChanged_}" ?readonly="${this.dialogIsReadonly_ || this.isFromEnterpriseShortcut_(this.dialogSource_)}">
      </cr-input>
    </div>
    <div slot="button-container">
      <cr-button class="cancel-button" @click="${this.onDialogCancelClick_}"
          ?hidden="${this.dialogIsReadonly_}">
        ${this.i18n("linkCancel")}
      </cr-button>
      <cr-button class="action-button" @click="${this.onSaveClick_}"
          ?disabled="${this.dialogSaveDisabled_}">
        ${this.i18n("linkDone")}
      </cr-button>
    </div>
  </cr-dialog>
  <cr-action-menu id="actionMenu">
    <button id="actionMenuViewOrEdit" class="dropdown-item"
        @click="${this.onViewOrEditClick_}">
      ${this.actionMenuViewOrEditTitle_}
    </button>
    <button id="actionMenuRemove" class="dropdown-item"
        @click="${this.onRemoveClick_}"
        ?disabled="${this.actionMenuRemoveDisabled_}">
      ${this.i18n("linkRemove")}
    </button>
  </cr-action-menu>
</div>
<cr-toast-manager id="toastManager" duration="10000">
  <cr-button id="undo" aria-label="${this.i18n("undoDescription")}"
      @click="${this.onUndoClick_}">
    ${this.i18n("undo")}
  </cr-button>
  <cr-button id="restore" aria-label="${this.getRestoreButtonText_()}"
      @click="${this.onRestoreDefaultsClick_}">
    ${this.getRestoreButtonText_()}
  </cr-button>
</cr-toast-manager>
<!--_html_template_end_-->`;
}
var MostVisitedWindowProxy = class _MostVisitedWindowProxy {
  matchMedia(query) {
    return window.matchMedia(query);
  }
  now() {
    return Date.now();
  }
  static getInstance() {
    return instance || (instance = new _MostVisitedWindowProxy());
  }
  static setInstance(obj) {
    instance = obj;
  }
};
var instance = null;
function resetTilePosition(tile) {
  tile.style.position = "";
  tile.style.left = "";
  tile.style.top = "";
}
function setTilePosition(tile, { x: x2, y: y3 }) {
  tile.style.position = "fixed";
  tile.style.left = `${x2}px`;
  tile.style.top = `${y3}px`;
}
function getHitIndex(rects, x2, y3) {
  return rects.findIndex(
    (r5) => x2 >= r5.left && x2 <= r5.right && y3 >= r5.top && y3 <= r5.bottom
  );
}
function normalizeUrl2(urlString) {
  try {
    const url = new URL(
      urlString.includes("://") ? urlString : `https://${urlString}/`
    );
    if (["http:", "https:"].includes(url.protocol)) {
      return url;
    }
  } catch (e5) {
  }
  return null;
}
var MostVisitedElementBase = I18nMixinLit(CrLitElement);
var MostVisitedElement = class extends MostVisitedElementBase {
  static get is() {
    return "cr-most-visited";
  }
  static get styles() {
    return getCss19();
  }
  render() {
    return getHtml12.bind(this)();
  }
  static get properties() {
    return {
      theme: { type: Object },
      /**
       * If true, disables editing/removing shortcuts, hides action buttons and
       * the add shortcut button, and prevents tile dragging.
       */
      nonEditable: {
        type: Boolean,
        reflect: true
      },
      /** If true, hides the text title under each tile/button. */
      hideTitle: {
        type: Boolean,
        reflect: true
      },
      /**
       * If true, renders MV tiles in a single row up to 10 columns wide.
       * If false, renders MV tiles in up to 2 rows up to 5 columns wide.
       */
      singleRow: { type: Boolean },
      /** If true, reflows tiles that are overflowing. */
      reflowOnOverflow: { type: Boolean },
      /**
       * When the tile icon background is dark, the icon color is white for
       * contrast. This can be used to determine the color of the tile hover as
       * well.
       */
      useWhiteTileIcon_: {
        type: Boolean,
        reflect: true
      },
      columnCount_: { type: Number, state: true },
      rowCount_: { type: Number, state: true },
      customLinksEnabled_: {
        type: Boolean,
        reflect: true
      },
      enterpriseShortcutsEnabled_: {
        type: Boolean,
        reflect: true
      },
      dialogTileTitle_: { type: String, state: true },
      dialogTileUrl_: { type: String, state: true },
      dialogTileUrlInvalid_: { type: Boolean, state: true },
      dialogTitle_: { type: String, state: true },
      dialogSaveDisabled_: { type: Boolean, state: true },
      dialogShortcutAlreadyExists_: { type: Boolean, state: true },
      dialogTileUrlError_: { type: String, state: true },
      dialogIsReadonly_: { type: Boolean, state: true },
      dialogSource_: { type: Number, state: true },
      info_: { type: Object, state: true },
      actionMenuRemoveDisabled_: { type: Boolean, state: true },
      actionMenuViewOrEditTitle_: { type: String, state: true },
      isDark_: {
        type: Boolean,
        reflect: true
      },
      /**
       * Used to hide hover style and cr-icon-button of tiles while the tiles
       * are being reordered.
       */
      reordering_: {
        type: Boolean,
        reflect: true
      },
      maxTiles_: { type: Number, state: true },
      maxVisibleTiles_: { type: Number, state: true },
      showAdd_: { type: Boolean, state: true },
      maxVisibleColumnCount_: { type: Number, state: true },
      tiles_: { type: Array, state: true },
      toastSource_: { type: Number, state: true },
      expandableTilesEnabled: { type: Boolean, reflect: true },
      maxTilesInCollapsedState: { type: Number, reflect: true },
      maxShortcutsInExpandedState: { type: Number, reflect: true },
      maxMostVisitedTilesInExpandedState: { type: Number, reflect: true },
      maxEnterpriseShortcuts: { type: Number, reflect: true },
      /**
       * If greater than 0, caps the total number of tiles to this value.
       */
      maxTiles: { type: Number, reflect: true },
      showAll_: { type: Boolean, state: true },
      showShowMore_: { type: Boolean, state: true },
      showShowLess_: { type: Boolean, state: true },
      visible_: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #theme = null;
  get theme() {
    return this.#theme;
  }
  set theme(_2) {
    this.#theme = _2;
  }
  #nonEditable = false;
  get nonEditable() {
    return this.#nonEditable;
  }
  set nonEditable(_2) {
    this.#nonEditable = _2;
  }
  #hideTitle = false;
  get hideTitle() {
    return this.#hideTitle;
  }
  set hideTitle(_2) {
    this.#hideTitle = _2;
  }
  #reflowOnOverflow = false;
  get reflowOnOverflow() {
    return this.#reflowOnOverflow;
  }
  set reflowOnOverflow(_2) {
    this.#reflowOnOverflow = _2;
  }
  #singleRow = false;
  get singleRow() {
    return this.#singleRow;
  }
  set singleRow(_2) {
    this.#singleRow = _2;
  }
  #expandableTilesEnabled = false;
  get expandableTilesEnabled() {
    return this.#expandableTilesEnabled;
  }
  set expandableTilesEnabled(_2) {
    this.#expandableTilesEnabled = _2;
  }
  #maxTilesInCollapsedState = 6;
  get maxTilesInCollapsedState() {
    return this.#maxTilesInCollapsedState;
  }
  set maxTilesInCollapsedState(_2) {
    this.#maxTilesInCollapsedState = _2;
  }
  #maxShortcutsInExpandedState = 10;
  get maxShortcutsInExpandedState() {
    return this.#maxShortcutsInExpandedState;
  }
  set maxShortcutsInExpandedState(_2) {
    this.#maxShortcutsInExpandedState = _2;
  }
  #maxMostVisitedTilesInExpandedState = 8;
  get maxMostVisitedTilesInExpandedState() {
    return this.#maxMostVisitedTilesInExpandedState;
  }
  set maxMostVisitedTilesInExpandedState(_2) {
    this.#maxMostVisitedTilesInExpandedState = _2;
  }
  #maxEnterpriseShortcuts = 10;
  get maxEnterpriseShortcuts() {
    return this.#maxEnterpriseShortcuts;
  }
  set maxEnterpriseShortcuts(_2) {
    this.#maxEnterpriseShortcuts = _2;
  }
  #maxTiles = 0;
  get maxTiles() {
    return this.#maxTiles;
  }
  set maxTiles(_2) {
    this.#maxTiles = _2;
  }
  #showAll_ = false;
  get showAll_() {
    return this.#showAll_;
  }
  set showAll_(_2) {
    this.#showAll_ = _2;
  }
  #showShowMore_ = false;
  get showShowMore_() {
    return this.#showShowMore_;
  }
  set showShowMore_(_2) {
    this.#showShowMore_ = _2;
  }
  #showShowLess_ = false;
  get showShowLess_() {
    return this.#showShowLess_;
  }
  set showShowLess_(_2) {
    this.#showShowLess_ = _2;
  }
  #useWhiteTileIcon_ = false;
  get useWhiteTileIcon_() {
    return this.#useWhiteTileIcon_;
  }
  set useWhiteTileIcon_(_2) {
    this.#useWhiteTileIcon_ = _2;
  }
  #columnCount_ = 3;
  get columnCount_() {
    return this.#columnCount_;
  }
  set columnCount_(_2) {
    this.#columnCount_ = _2;
  }
  #rowCount_ = 1;
  get rowCount_() {
    return this.#rowCount_;
  }
  set rowCount_(_2) {
    this.#rowCount_ = _2;
  }
  #customLinksEnabled_ = false;
  get customLinksEnabled_() {
    return this.#customLinksEnabled_;
  }
  set customLinksEnabled_(_2) {
    this.#customLinksEnabled_ = _2;
  }
  #enterpriseShortcutsEnabled_ = false;
  get enterpriseShortcutsEnabled_() {
    return this.#enterpriseShortcutsEnabled_;
  }
  set enterpriseShortcutsEnabled_(_2) {
    this.#enterpriseShortcutsEnabled_ = _2;
  }
  #dialogTileTitle_ = "";
  get dialogTileTitle_() {
    return this.#dialogTileTitle_;
  }
  set dialogTileTitle_(_2) {
    this.#dialogTileTitle_ = _2;
  }
  #dialogTileUrl_ = "";
  get dialogTileUrl_() {
    return this.#dialogTileUrl_;
  }
  set dialogTileUrl_(_2) {
    this.#dialogTileUrl_ = _2;
  }
  #dialogTileUrlInvalid_ = false;
  get dialogTileUrlInvalid_() {
    return this.#dialogTileUrlInvalid_;
  }
  set dialogTileUrlInvalid_(_2) {
    this.#dialogTileUrlInvalid_ = _2;
  }
  #dialogTitle_ = "";
  get dialogTitle_() {
    return this.#dialogTitle_;
  }
  set dialogTitle_(_2) {
    this.#dialogTitle_ = _2;
  }
  #dialogSaveDisabled_ = true;
  get dialogSaveDisabled_() {
    return this.#dialogSaveDisabled_;
  }
  set dialogSaveDisabled_(_2) {
    this.#dialogSaveDisabled_ = _2;
  }
  #dialogShortcutAlreadyExists_ = false;
  get dialogShortcutAlreadyExists_() {
    return this.#dialogShortcutAlreadyExists_;
  }
  set dialogShortcutAlreadyExists_(_2) {
    this.#dialogShortcutAlreadyExists_ = _2;
  }
  #dialogTileUrlError_ = "";
  get dialogTileUrlError_() {
    return this.#dialogTileUrlError_;
  }
  set dialogTileUrlError_(_2) {
    this.#dialogTileUrlError_ = _2;
  }
  #dialogIsReadonly_ = false;
  get dialogIsReadonly_() {
    return this.#dialogIsReadonly_;
  }
  set dialogIsReadonly_(_2) {
    this.#dialogIsReadonly_ = _2;
  }
  #dialogSource_ = TileSource.CUSTOM_LINKS;
  get dialogSource_() {
    return this.#dialogSource_;
  }
  set dialogSource_(_2) {
    this.#dialogSource_ = _2;
  }
  #actionMenuRemoveDisabled_ = false;
  get actionMenuRemoveDisabled_() {
    return this.#actionMenuRemoveDisabled_;
  }
  set actionMenuRemoveDisabled_(_2) {
    this.#actionMenuRemoveDisabled_ = _2;
  }
  #actionMenuViewOrEditTitle_ = "";
  get actionMenuViewOrEditTitle_() {
    return this.#actionMenuViewOrEditTitle_;
  }
  set actionMenuViewOrEditTitle_(_2) {
    this.#actionMenuViewOrEditTitle_ = _2;
  }
  #isDark_ = false;
  get isDark_() {
    return this.#isDark_;
  }
  set isDark_(_2) {
    this.#isDark_ = _2;
  }
  #reordering_ = false;
  get reordering_() {
    return this.#reordering_;
  }
  set reordering_(_2) {
    this.#reordering_ = _2;
  }
  #maxTiles_ = 0;
  get maxTiles_() {
    return this.#maxTiles_;
  }
  set maxTiles_(_2) {
    this.#maxTiles_ = _2;
  }
  #maxVisibleTiles_ = 0;
  get maxVisibleTiles_() {
    return this.#maxVisibleTiles_;
  }
  set maxVisibleTiles_(_2) {
    this.#maxVisibleTiles_ = _2;
  }
  #showAdd_ = false;
  get showAdd_() {
    return this.#showAdd_;
  }
  set showAdd_(_2) {
    this.#showAdd_ = _2;
  }
  #maxVisibleColumnCount_ = 0;
  get maxVisibleColumnCount_() {
    return this.#maxVisibleColumnCount_;
  }
  set maxVisibleColumnCount_(_2) {
    this.#maxVisibleColumnCount_ = _2;
  }
  #tiles_ = [];
  get tiles_() {
    return this.#tiles_;
  }
  set tiles_(_2) {
    this.#tiles_ = _2;
  }
  #toastSource_ = TileSource.CUSTOM_LINKS;
  get toastSource_() {
    return this.#toastSource_;
  }
  set toastSource_(_2) {
    this.#toastSource_ = _2;
  }
  #visible_ = false;
  get visible_() {
    return this.#visible_;
  }
  set visible_(_2) {
    this.#visible_ = _2;
  }
  adding_ = false;
  browserProxy_;
  windowProxy_;
  actionMenuTargetIndex_ = -1;
  dragOffset_;
  tileRects_ = [];
  isRtl_ = false;
  mediaEventTracker_;
  eventTracker_;
  boundOnDocumentKeyDown_ = (_e) => null;
  prefetchTimer_ = null;
  preconnectTimer_ = null;
  dragImage_;
  mostVisitedHighDpiFaviconsEnabled_ = loadTimeData.getBoolean("mostVisitedHighDpiFaviconsEnabled");
  #info_ = null;
  get info_() {
    return this.#info_;
  }
  set info_(_2) {
    this.#info_ = _2;
  }
  get tileElements_() {
    return Array.from(
      this.shadowRoot.querySelectorAll(".tile:not([hidden])")
    );
  }
  constructor() {
    performance.mark("most-visited-creation-start");
    super();
    this.browserProxy_ = browserProxyFactory.getInstance();
    this.windowProxy_ = MostVisitedWindowProxy.getInstance();
    this.dragOffset_ = null;
    this.dragImage_ = new Image(1, 1);
    this.dragImage_.src = "data:image/gif;base64,R0lGODlhAQABAAAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
    this.mediaEventTracker_ = new EventTracker();
    this.eventTracker_ = new EventTracker();
  }
  connectedCallback() {
    super.connectedCallback();
    this.isRtl_ = window.getComputedStyle(this)["direction"] === "rtl";
    this.onSingleRowChange_();
    this.browserProxy_.callbackRouter.setMostVisitedInfo.addListener((info) => {
      performance.measure("most-visited-mojo", "most-visited-mojo-start");
      this.info_ = info;
    });
    this.browserProxy_.callbackRouter.onMostVisitedTilesAutoRemoval.addListener(
      () => {
        this.autoRemovalToast_();
      }
    );
    this.browserProxy_.handler.getMostVisitedExpandedState().then(
      ({ isExpanded }) => {
        this.showAll_ = isExpanded;
      }
    );
    performance.mark("most-visited-mojo-start");
    this.eventTracker_.add(document, "visibilitychange", () => {
      if (document.visibilityState === "visible") {
        this.browserProxy_.handler.updateMostVisitedInfo();
      }
    });
    this.browserProxy_.handler.updateMostVisitedInfo();
    FocusOutlineManager.forDocument(document);
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    this.mediaEventTracker_.removeAll();
    this.eventTracker_.removeAll();
    this.ownerDocument.removeEventListener(
      "keydown",
      this.boundOnDocumentKeyDown_
    );
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    const changedPrivateProperties = changedProperties;
    if (changedProperties.has("theme")) {
      this.useWhiteTileIcon_ = this.computeUseWhiteTileIcon_();
      this.isDark_ = this.computeIsDark_();
    }
    if (changedPrivateProperties.has("info_") && this.info_ !== null) {
      this.visible_ = this.info_.visible;
      this.customLinksEnabled_ = this.info_.customLinksEnabled;
      this.enterpriseShortcutsEnabled_ = this.info_.enterpriseShortcutsEnabled;
      const totalMax = (this.customLinksEnabled_ ? this.maxShortcutsInExpandedState : this.maxMostVisitedTilesInExpandedState) + (this.enterpriseShortcutsEnabled_ ? this.maxEnterpriseShortcuts : 0);
      this.maxTiles_ = this.maxTiles > 0 ? Math.min(this.maxTiles, totalMax) : totalMax;
      this.tiles_ = this.info_.tiles.slice(0, this.maxTiles_);
    }
    this.showShowMore_ = this.computeShowShowMore_();
    this.showShowLess_ = this.computeShowShowLess_();
    this.showAdd_ = this.computeShowAdd_();
    this.columnCount_ = this.computeColumnCount_();
    this.rowCount_ = this.computeRowCount_();
    if (changedPrivateProperties.has("tiles_") || changedPrivateProperties.has("dialogTileUrl_")) {
      this.dialogShortcutAlreadyExists_ = this.computeDialogShortcutAlreadyExists_();
    }
    if (changedPrivateProperties.has("dialogShortcutAlreadyExists_")) {
      this.dialogTileUrlError_ = this.computeDialogTileUrlError_();
    }
    if (changedPrivateProperties.has("dialogTitle_") || changedPrivateProperties.has("dialogTileUrl_")) {
      this.dialogSaveDisabled_ = this.computeDialogSaveDisabled_();
    }
  }
  firstUpdated() {
    this.boundOnDocumentKeyDown_ = (e5) => this.onDocumentKeyDown_(e5);
    this.ownerDocument.addEventListener(
      "keydown",
      this.boundOnDocumentKeyDown_
    );
    performance.measure("most-visited-creation", "most-visited-creation-start");
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    this.maxVisibleTiles_ = this.computeMaxVisibleTiles_();
    const changedPrivateProperties = changedProperties;
    if (changedProperties.has("singleRow")) {
      this.onSingleRowChange_();
    }
    if (changedPrivateProperties.has("tiles_")) {
      if (this.tiles_.length > 0) {
        this.onTilesRendered_();
      }
    }
  }
  getBackgroundColorStyle_() {
    const skColor = this.theme ? this.theme.backgroundColor : null;
    return skColor ? skColorToRgba(skColor) : "inherit";
  }
  // Adds "force-hover" class to the tile element positioned at `index`.
  enableForceHover_(index) {
    this.tileElements_[index].classList.add("force-hover");
  }
  clearForceHover_() {
    const forceHover = this.shadowRoot.querySelector(".force-hover");
    if (forceHover) {
      forceHover.classList.remove("force-hover");
    }
  }
  computeColumnCount_() {
    const shortcutCount = this.tiles_ ? this.tiles_.length : 0;
    const canShowAdd = this.expandableTilesEnabled ? this.showAdd_ : !this.nonEditable && this.maxTiles_ > shortcutCount;
    const canShowShowMore = this.expandableTilesEnabled && this.showShowMore_;
    const canShowShowLess = this.expandableTilesEnabled && this.showShowLess_;
    const visibleShortcutCount = canShowShowMore ? this.maxTilesInCollapsedState : shortcutCount;
    const totalTileCount = visibleShortcutCount + (canShowAdd ? 1 : 0) + (canShowShowMore || canShowShowLess ? 1 : 0);
    const columnCount = totalTileCount <= this.maxVisibleColumnCount_ ? totalTileCount : Math.min(
      this.maxVisibleColumnCount_,
      Math.ceil(totalTileCount / (this.singleRow ? 1 : 2))
    );
    return columnCount || 3;
  }
  computeRowCount_() {
    if (this.columnCount_ === 0) {
      return 0;
    }
    if (this.reflowOnOverflow && this.tiles_) {
      const visibleShortcutCount = this.expandableTilesEnabled && this.showShowMore_ ? this.maxTilesInCollapsedState : this.tiles_.length;
      return Math.ceil(
        (visibleShortcutCount + (this.showAdd_ ? 1 : 0) + (this.showShowMore_ || this.showShowLess_ ? 1 : 0)) / this.columnCount_
      );
    }
    if (this.singleRow) {
      return 1;
    }
    const shortcutCount = this.tiles_ ? this.tiles_.length : 0;
    return this.columnCount_ <= shortcutCount ? 2 : 1;
  }
  computeMaxVisibleTiles_() {
    if (this.expandableTilesEnabled && this.showShowMore_) {
      return this.maxTilesInCollapsedState;
    }
    if (this.reflowOnOverflow) {
      return this.maxTiles_;
    }
    return this.columnCount_ * this.rowCount_;
  }
  computeShowAdd_() {
    if (this.nonEditable || this.showShowMore_) {
      return false;
    }
    if (!this.customLinksEnabled_) {
      return false;
    }
    const customLinkTilesCount = this.tiles_.filter((tile) => !this.isFromEnterpriseShortcut_(tile.source)).length;
    return this.tiles_.length < (this.expandableTilesEnabled && this.showAll_ ? this.maxTiles_ : this.maxVisibleTiles_) && customLinkTilesCount < this.maxShortcutsInExpandedState;
  }
  computeShowShowMore_() {
    return this.expandableTilesEnabled && !this.showAll_ && this.tiles_ && this.tiles_.length >= this.maxTilesInCollapsedState;
  }
  computeShowShowLess_() {
    return this.expandableTilesEnabled && this.showAll_ && this.tiles_ && this.tiles_.length >= this.maxTilesInCollapsedState;
  }
  async onShowMoreClick_() {
    this.showAll_ = true;
    this.browserProxy_.handler.setMostVisitedExpandedState(this.showAll_);
    await this.updateComplete;
    this.tileFocus_(this.maxTilesInCollapsedState);
  }
  async onShowLessClick_() {
    this.showAll_ = false;
    this.browserProxy_.handler.setMostVisitedExpandedState(this.showAll_);
    await this.updateComplete;
    this.$.showMore.focus();
  }
  computeDialogSaveDisabled_() {
    return !this.dialogTileUrl_.trim() || normalizeUrl2(this.dialogTileUrl_) === null || this.dialogShortcutAlreadyExists_;
  }
  computeDialogShortcutAlreadyExists_() {
    const dialogTileHref = (normalizeUrl2(this.dialogTileUrl_) || {}).href;
    if (!dialogTileHref) {
      return false;
    }
    if (this.dialogSource_ === TileSource.ENTERPRISE_SHORTCUTS) {
      return false;
    }
    return (this.tiles_ || []).some(({ url }, index) => {
      if (index === this.actionMenuTargetIndex_) {
        return false;
      }
      const otherUrl = normalizeUrl2(url);
      return otherUrl && otherUrl.href === dialogTileHref && this.tiles_[index].source !== TileSource.ENTERPRISE_SHORTCUTS;
    });
  }
  computeDialogTileUrlError_() {
    return loadTimeData.getString(
      this.dialogShortcutAlreadyExists_ ? "shortcutAlreadyExists" : "invalidUrl"
    );
  }
  computeIsDark_() {
    return this.theme ? this.theme.isDark : false;
  }
  computeUseWhiteTileIcon_() {
    return this.theme ? this.theme.useWhiteTileIcon : false;
  }
  /**
   * This method is always called when the drag and drop was finished (even when
   * the drop was canceled). If the tiles were reordered successfully, there
   * should be a tile with the "dropped" class.
   *
   * |reordering_| is not set to false when the tiles are reordered. The callers
   * will need to set it to false. This is necessary to handle a mouse drag
   * issue.
   */
  dragEnd_() {
    if (!this.customLinksEnabled_ && !this.enterpriseShortcutsEnabled_) {
      this.reordering_ = false;
      return;
    }
    this.dragOffset_ = null;
    const dragElement = this.shadowRoot.querySelector(".tile.dragging");
    const droppedElement = this.shadowRoot.querySelector(".tile.dropped");
    if (!dragElement && !droppedElement) {
      this.reordering_ = false;
      return;
    }
    if (dragElement) {
      dragElement.classList.remove("dragging");
      this.tileElements_.forEach((el) => resetTilePosition(el));
      resetTilePosition(this.$.addShortcut);
      resetTilePosition(this.$.showMore);
      resetTilePosition(this.$.showLess);
    } else if (droppedElement) {
      droppedElement.classList.remove("dropped");
    }
  }
  /**
   * This method is called on "drop" events (i.e. when the user drops the tile
   * on a valid region.)
   *
   * If a pointer is over a tile rect that is different from the one being
   * dragged, the dragging tile is moved to the new position. The reordering is
   * done in the DOM and by the |reorderMostVisitedTile()| call. This is done to
   * prevent flicking between the time when the tiles are moved back to their
   * original positions (by removing position absolute) and when the tiles are
   * updated via the |setMostVisitedInfo| handler.
   *
   * We remove the "dragging" class in this method, and add "dropped" to
   * indicate that the dragged tile was successfully dropped.
   */
  drop_(x2, y3) {
    if (!this.customLinksEnabled_ && !this.enterpriseShortcutsEnabled_) {
      return;
    }
    const dragElement = this.shadowRoot.querySelector(".tile.dragging");
    if (!dragElement) {
      return;
    }
    const dragIndex = Number(dragElement.dataset["index"]);
    const dropIndex = getHitIndex(this.tileRects_, x2, y3);
    if (dragIndex !== dropIndex && dropIndex > -1) {
      const dragTile = this.tiles_[dragIndex];
      assert(dragTile);
      const dropTile = this.tiles_[dropIndex];
      assert(dropTile);
      if (this.isFromEnterpriseShortcut_(dragTile.source) !== this.isFromEnterpriseShortcut_(dropTile.source)) {
        return;
      }
      const [draggingTile] = this.tiles_.splice(dragIndex, 1);
      assert(draggingTile);
      this.tiles_.splice(dropIndex, 0, draggingTile);
      this.requestUpdate();
      let newDropIndex = dropIndex;
      if (!this.isFromEnterpriseShortcut_(draggingTile.source)) {
        newDropIndex -= this.tiles_.filter((t5) => this.isFromEnterpriseShortcut_(t5.source)).length;
      }
      this.browserProxy_.handler.reorderMostVisitedTile(
        draggingTile,
        newDropIndex
      );
      dragElement.classList.remove("dragging");
      dragElement.classList.add("dropped");
      this.tileElements_.forEach((el) => resetTilePosition(el));
      resetTilePosition(this.$.addShortcut);
      resetTilePosition(this.$.showMore);
      resetTilePosition(this.$.showLess);
    }
  }
  /**
   * The positions of the tiles are updated based on the location of the
   * pointer.
   */
  dragOver_(x2, y3) {
    const dragElement = this.shadowRoot.querySelector(".tile.dragging");
    if (!dragElement) {
      this.reordering_ = false;
      return;
    }
    const dragIndex = Number(dragElement.dataset["index"]);
    setTilePosition(dragElement, {
      x: x2 - this.dragOffset_.x,
      y: y3 - this.dragOffset_.y
    });
    let dropIndex = getHitIndex(this.tileRects_, x2, y3);
    if (dropIndex > -1) {
      const dragTile = this.tiles_[dragIndex];
      const dropTile = this.tiles_[dropIndex];
      if (this.isFromEnterpriseShortcut_(dragTile.source) !== this.isFromEnterpriseShortcut_(dropTile.source)) {
        dropIndex = -1;
      }
    }
    this.tileElements_.forEach((element, i7) => {
      let positionIndex;
      if (i7 === dragIndex) {
        return;
      } else if (dropIndex === -1) {
        positionIndex = i7;
      } else if (dragIndex < dropIndex && dragIndex <= i7 && i7 <= dropIndex) {
        positionIndex = i7 - 1;
      } else if (dragIndex > dropIndex && dragIndex >= i7 && i7 >= dropIndex) {
        positionIndex = i7 + 1;
      } else {
        positionIndex = i7;
      }
      setTilePosition(element, this.tileRects_[positionIndex]);
    });
  }
  /**
   * Sets up tile reordering for both drag and touch events. This method stores
   * the following to be used in |dragOver_()| and |dragEnd_()|.
   *   |dragOffset_|: This is the mouse/touch offset with respect to the
   *       top/left corner of the tile being dragged. It is used to update the
   *       dragging tile location during the drag.
   *   |reordering_|: This is property/attribute used to hide the hover style
   *       and cr-icon-button of the tiles while they are being reordered.
   *   |tileRects_|: This is the rects of the tiles before the drag start. It is
   *       to determine which tile the pointer is over while dragging.
   */
  dragStart_(dragElement, x2, y3) {
    this.clearForceHover_();
    dragElement.classList.add("dragging");
    const dragElementRect = dragElement.getBoundingClientRect();
    this.dragOffset_ = {
      x: x2 - dragElementRect.x,
      y: y3 - dragElementRect.y
    };
    const visibleElements = this.tileElements_;
    const numTiles = visibleElements.length;
    if (this.showAdd_) {
      visibleElements.push(this.$.addShortcut);
    }
    if (this.showShowMore_) {
      visibleElements.push(this.$.showMore);
    }
    if (this.showShowLess_) {
      visibleElements.push(this.$.showLess);
    }
    const allRects = visibleElements.map((t5) => t5.getBoundingClientRect());
    this.tileRects_ = allRects.slice(0, numTiles);
    visibleElements.forEach((el, i7) => {
      setTilePosition(el, allRects[i7]);
    });
    this.reordering_ = true;
  }
  getFaviconUrl_(url) {
    return faviconUrl(url);
  }
  getRestoreButtonText_() {
    return loadTimeData.getString(
      this.isFromEnterpriseShortcut_(this.toastSource_) ? "restoreDefaultEnterpriseShortcuts" : this.customLinksEnabled_ ? "restoreDefaultLinks" : "restoreThumbnailsShort"
    );
  }
  getTileTitleDirectionClass_(tile) {
    return tile.titleDirection === TextDirection.RIGHT_TO_LEFT ? "title-rtl" : "title-ltr";
  }
  isHidden_(index) {
    if (this.reflowOnOverflow && !this.showShowMore_) {
      return false;
    }
    return index >= this.maxVisibleTiles_;
  }
  onSingleRowChange_() {
    if (!this.isConnected) {
      return;
    }
    this.mediaEventTracker_.removeAll();
    const queryLists = [];
    const updateCount = () => {
      const index = queryLists.findIndex((listener) => listener.matches);
      this.maxVisibleColumnCount_ = 3 + (index > -1 ? queryLists.length - index : 0);
    };
    const tileSize = parseInt(
      getComputedStyle(this).getPropertyValue("--most-visited-tile-size"),
      10
    ) || 112;
    const maxColumnCount = this.singleRow ? 10 : 5;
    for (let i7 = maxColumnCount; i7 >= 4; i7--) {
      const query = `(min-width: ${tileSize * (i7 + 1)}px)`;
      const queryList = this.windowProxy_.matchMedia(query);
      this.mediaEventTracker_.add(queryList, "change", updateCount);
      queryLists.push(queryList);
    }
    updateCount();
  }
  onAddClick_() {
    this.dialogIsReadonly_ = false;
    this.dialogSource_ = TileSource.CUSTOM_LINKS;
    this.dialogTitle_ = loadTimeData.getString("addLinkTitle");
    this.dialogTileTitle_ = "";
    this.dialogTileUrl_ = "";
    this.dialogTileUrlInvalid_ = false;
    this.adding_ = true;
    this.$.dialog.showModal();
  }
  onAddShortcutKeydown_(e5) {
    if (hasKeyModifiers(e5)) {
      return;
    }
    if (!this.tiles_ || this.tiles_.length === 0) {
      return;
    }
    const backKey = this.isRtl_ ? "ArrowRight" : "ArrowLeft";
    if (e5.key === backKey || e5.key === "ArrowUp") {
      this.tileFocus_(this.tiles_.length - 1);
    }
    const advanceKey = this.isRtl_ ? "ArrowLeft" : "ArrowRight";
    if (e5.key === advanceKey || e5.key === "ArrowDown") {
      if (this.showShowLess_) {
        this.$.showLess.focus();
      }
    }
  }
  onShowMoreKeydown_(e5) {
    if (hasKeyModifiers(e5)) {
      return;
    }
    const backKey = this.isRtl_ ? "ArrowRight" : "ArrowLeft";
    if (e5.key === backKey || e5.key === "ArrowUp") {
      this.tileFocus_(this.maxTilesInCollapsedState - 1);
    }
  }
  onShowLessKeydown_(e5) {
    if (hasKeyModifiers(e5)) {
      return;
    }
    const backKey = this.isRtl_ ? "ArrowRight" : "ArrowLeft";
    if (e5.key === backKey || e5.key === "ArrowUp") {
      if (this.showAdd_) {
        this.$.addShortcut.focus();
      } else {
        this.tileFocus_(this.tiles_.length - 1);
      }
    }
  }
  onDialogCancelClick_() {
    this.actionMenuTargetIndex_ = -1;
    this.$.dialog.cancel();
  }
  onDialogClose_() {
    this.dialogTileUrl_ = "";
    if (this.adding_) {
      this.$.addShortcut.focus();
    }
    this.adding_ = false;
  }
  onDialogKeydown_(e5) {
    if (e5.key !== "Tab") {
      return;
    }
    const focusable = Array.from(this.$.dialog.querySelectorAll(
      "cr-input, cr-button:not([disabled]):not([hidden])"
    ));
    if (focusable.length === 0) {
      return;
    }
    const firstEl = focusable[0];
    const lastEl = focusable[focusable.length - 1];
    const path = e5.composedPath();
    if (e5.shiftKey && path.includes(firstEl)) {
      e5.preventDefault();
      lastEl.focus();
    } else if (!e5.shiftKey && path.includes(lastEl)) {
      e5.preventDefault();
      firstEl.focus();
    }
  }
  onDialogTileUrlBlur_() {
    if (this.dialogTileUrl_.length > 0 && (normalizeUrl2(this.dialogTileUrl_) === null || this.dialogShortcutAlreadyExists_)) {
      this.dialogTileUrlInvalid_ = true;
    }
  }
  onDialogTileUrlValueChanged_(e5) {
    this.dialogTileUrl_ = e5.target.value;
    this.dialogTileUrlInvalid_ = false;
  }
  onDialogTileNameValueChanged_(e5) {
    this.dialogTileTitle_ = e5.target.value;
  }
  onDocumentKeyDown_(e5) {
    if (this.nonEditable || e5.altKey || e5.shiftKey) {
      return;
    }
    const modifier = isMac ? e5.metaKey && !e5.ctrlKey : e5.ctrlKey && !e5.metaKey;
    if (modifier && e5.key === "z") {
      if (this.onUndoClick_()) {
        e5.preventDefault();
      }
    }
  }
  onDragstart_(e5) {
    if (this.nonEditable) {
      return;
    }
    const item = this.tiles_[this.getCurrentTargetIndex_(e5)];
    assert(item);
    if (!this.customLinksEnabled_ && !this.isFromEnterpriseShortcut_(item.source)) {
      return;
    }
    if (e5.dataTransfer) {
      e5.dataTransfer.setDragImage(this.dragImage_, 0, 0);
    }
    this.dragStart_(e5.target, e5.x, e5.y);
    const dragOver = (e22) => {
      e22.preventDefault();
      e22.dataTransfer.dropEffect = "move";
      this.dragOver_(e22.x, e22.y);
    };
    const drop = (e22) => {
      this.drop_(e22.x, e22.y);
      const dropIndex = getHitIndex(this.tileRects_, e22.x, e22.y);
      if (dropIndex !== -1) {
        this.enableForceHover_(dropIndex);
      }
    };
    this.ownerDocument.addEventListener("dragover", dragOver);
    this.ownerDocument.addEventListener("drop", drop);
    this.ownerDocument.addEventListener("dragend", (_2) => {
      this.ownerDocument.removeEventListener("dragover", dragOver);
      this.ownerDocument.removeEventListener("drop", drop);
      this.dragEnd_();
      this.addEventListener("pointermove", () => {
        this.clearForceHover_();
        this.reordering_ = false;
      }, { once: true });
    }, { once: true });
  }
  onViewOrEditClick_() {
    this.$.actionMenu.close();
    const tile = this.tiles_[this.actionMenuTargetIndex_];
    const isReadonly = !tile.allowUserEdit;
    this.dialogIsReadonly_ = isReadonly;
    this.dialogSource_ = tile.source;
    this.dialogTitle_ = loadTimeData.getString(isReadonly ? "viewLinkTitle" : "editLinkTitle");
    this.dialogTileTitle_ = tile.title;
    this.dialogTileUrl_ = tile.url;
    this.dialogTileUrlInvalid_ = false;
    this.$.dialog.showModal();
  }
  onRestoreDefaultsClick_() {
    if (!this.$.toastManager.isToastOpen || this.$.toastManager.slottedHidden) {
      return;
    }
    this.$.toastManager.hide();
    this.browserProxy_.handler.restoreMostVisitedDefaults(this.toastSource_);
  }
  async onRemoveClick_() {
    this.$.actionMenu.close();
    await this.tileRemove_(this.actionMenuTargetIndex_);
    this.actionMenuTargetIndex_ = -1;
  }
  async onSaveClick_() {
    if (this.dialogIsReadonly_) {
      this.$.dialog.close();
      return;
    }
    const newUrl = normalizeUrl2(this.dialogTileUrl_).href;
    this.$.dialog.close();
    let newTitle = this.dialogTileTitle_.trim();
    if (newTitle.length === 0) {
      newTitle = this.dialogTileUrl_;
    }
    if (this.adding_) {
      const { success } = await this.browserProxy_.handler.addMostVisitedTile(newUrl, newTitle);
      this.toast_(
        success ? "linkAddedMsg" : "linkCantCreate",
        success,
        TileSource.TOP_SITES
      );
    } else {
      const oldTile = this.tiles_[this.actionMenuTargetIndex_];
      if (oldTile.url !== newUrl || oldTile.title !== newTitle) {
        const { success } = await this.browserProxy_.handler.updateMostVisitedTile(
          oldTile,
          newUrl,
          newTitle
        );
        this.toast_(
          success ? "linkEditedMsg" : "linkCantEdit",
          success,
          oldTile.source
        );
      }
      this.actionMenuTargetIndex_ = -1;
    }
  }
  getCurrentTargetIndex_(e5) {
    const target = e5.currentTarget;
    return Number(target.dataset["index"]);
  }
  onTileActionButtonClick_(e5) {
    e5.preventDefault();
    this.actionMenuTargetIndex_ = this.getCurrentTargetIndex_(e5);
    const item = this.tiles_[this.getCurrentTargetIndex_(e5)];
    assert(item);
    this.actionMenuRemoveDisabled_ = !item.allowUserDelete;
    this.actionMenuViewOrEditTitle_ = loadTimeData.getString(
      item.allowUserEdit ? "editLinkTitle" : "viewLink"
    );
    this.$.actionMenu.showAt(e5.target);
  }
  onTileRemoveButtonClick_(e5) {
    e5.preventDefault();
    this.tileRemove_(this.getCurrentTargetIndex_(e5));
  }
  onTileClick_(e5) {
    if (e5.defaultPrevented) {
      return;
    }
    e5.preventDefault();
    const index = this.getCurrentTargetIndex_(e5);
    const item = this.tiles_[index];
    this.browserProxy_.handler.onMostVisitedTileNavigation(
      item,
      index,
      e5.button || 0,
      e5.altKey,
      e5.ctrlKey,
      e5.metaKey,
      e5.shiftKey
    );
  }
  onTileKeydown_(e5) {
    if (hasKeyModifiers(e5)) {
      return;
    }
    if (e5.key !== "ArrowLeft" && e5.key !== "ArrowRight" && e5.key !== "ArrowUp" && e5.key !== "ArrowDown" && e5.key !== "Delete") {
      return;
    }
    const index = this.getCurrentTargetIndex_(e5);
    if (e5.key === "Delete") {
      if (!this.nonEditable) {
        this.tileRemove_(index);
      }
      return;
    }
    const advanceKey = this.isRtl_ ? "ArrowLeft" : "ArrowRight";
    const delta = e5.key === advanceKey || e5.key === "ArrowDown" ? 1 : -1;
    const newIndex = Math.max(0, index + delta);
    if (this.showShowMore_ && newIndex === this.maxTilesInCollapsedState) {
      this.$.showMore.focus();
    } else {
      this.tileFocus_(newIndex);
    }
  }
  onTileMouseenter_(e5) {
    if (e5.defaultPrevented) {
      return;
    }
    const item = this.tiles_[this.getCurrentTargetIndex_(e5)];
    assert(item);
    if (loadTimeData.getBoolean("prerenderOnPressEnabled") && loadTimeData.getInteger("preconnectStartTimeThreshold") >= 0) {
      this.preconnectTimer_ = setTimeout(() => {
        this.browserProxy_.handler.preconnectMostVisitedTile(item);
      }, loadTimeData.getInteger("preconnectStartTimeThreshold"));
    }
    if (loadTimeData.getBoolean("prefetchTriggerEnabled") && loadTimeData.getInteger("prefetchStartTimeThreshold") >= 0) {
      this.prefetchTimer_ = setTimeout(() => {
        this.browserProxy_.handler.prefetchMostVisitedTile(item);
      }, loadTimeData.getInteger("prefetchStartTimeThreshold"));
    }
  }
  onTileMousedown_(e5) {
    if (e5.defaultPrevented) {
      return;
    }
    if (loadTimeData.getBoolean("prerenderOnPressEnabled")) {
      const item = this.tiles_[this.getCurrentTargetIndex_(e5)];
      assert(item);
      if (loadTimeData.getBoolean("prefetchTriggerEnabled")) {
        this.browserProxy_.handler.prefetchMostVisitedTile(item);
      }
      this.browserProxy_.handler.prerenderMostVisitedTile(item);
    }
  }
  onTileMouseleave_(e5) {
    if (e5.defaultPrevented) {
      return;
    }
    if (this.prefetchTimer_) {
      clearTimeout(this.prefetchTimer_);
    }
    if (this.preconnectTimer_) {
      clearTimeout(this.preconnectTimer_);
    }
    if (loadTimeData.getBoolean("prerenderOnPressEnabled")) {
      this.browserProxy_.handler.cancelPrerender();
    }
  }
  onUndoClick_() {
    if (!this.$.toastManager.isToastOpen || this.$.toastManager.slottedHidden) {
      return false;
    }
    this.$.toastManager.hide();
    this.browserProxy_.handler.undoMostVisitedTileAction(this.toastSource_);
    return true;
  }
  onTouchstart_(e5) {
    if (this.nonEditable || this.reordering_) {
      return;
    }
    const item = this.tiles_[this.getCurrentTargetIndex_(e5)];
    assert(item);
    if (!this.customLinksEnabled_ && !this.isFromEnterpriseShortcut_(item.source)) {
      return;
    }
    const tileElement = e5.composedPath().find((el) => el.classList && el.classList.contains("tile"));
    if (!tileElement) {
      return;
    }
    const { clientX, clientY } = e5.changedTouches[0];
    this.dragStart_(tileElement, clientX, clientY);
    const touchMove = (e22) => {
      const { clientX: clientX2, clientY: clientY2 } = e22.changedTouches[0];
      this.dragOver_(clientX2, clientY2);
    };
    const touchEnd = (e22) => {
      this.ownerDocument.removeEventListener("touchmove", touchMove);
      tileElement.removeEventListener("touchend", touchEnd);
      tileElement.removeEventListener("touchcancel", touchEnd);
      const { clientX: clientX2, clientY: clientY2 } = e22.changedTouches[0];
      this.drop_(clientX2, clientY2);
      this.dragEnd_();
      this.reordering_ = false;
    };
    this.ownerDocument.addEventListener("touchmove", touchMove);
    tileElement.addEventListener("touchend", touchEnd, { once: true });
    tileElement.addEventListener("touchcancel", touchEnd, { once: true });
  }
  tileFocus_(index) {
    if (index < 0) {
      return;
    }
    const tileElements = this.tileElements_;
    if (index < tileElements.length) {
      tileElements[index].querySelector("a").focus();
    } else if (this.showAdd_ && index === tileElements.length) {
      this.$.addShortcut.focus();
    } else if (this.showShowLess_ && index === tileElements.length) {
      this.$.showLess.focus();
    }
  }
  autoRemovalToast_() {
    this.fire("most-visited-auto-removed", {
      message: loadTimeData.getString("shortcutsInactivityRemovalMsg"),
      undo: () => {
        this.browserProxy_.handler.undoMostVisitedAutoRemoval();
      }
    });
  }
  toast_(msgId, showButtons, source) {
    this.toastSource_ = source;
    this.$.toastManager.show(loadTimeData.getString(msgId), !showButtons);
  }
  async tileRemove_(index) {
    const tile = this.tiles_[index];
    this.browserProxy_.handler.deleteMostVisitedTile(tile);
    this.toast_(
      "linkRemovedMsg",
      /* showButtons= */
      this.customLinksEnabled_ || this.enterpriseShortcutsEnabled_ || !tile.isQueryTile,
      tile.source
    );
    await this.updateComplete;
    this.tileFocus_(index);
  }
  onTilesRendered_() {
    performance.measure("most-visited-rendered");
    assert(this.maxVisibleTiles_ > 0);
    this.browserProxy_.handler.onMostVisitedTilesRendered(
      this.tiles_.slice(0, this.maxVisibleTiles_),
      this.windowProxy_.now()
    );
  }
  getMoreActionText_(title) {
    return loadTimeData.getString("shortcutMoreActions") ? loadTimeData.getStringF("shortcutMoreActions", title) : "";
  }
  getRemoveButtonText_(title) {
    return this.i18n("linkRemoveA11y", htmlEscape(title));
  }
  isFromEnterpriseShortcut_(source) {
    return source === TileSource.ENTERPRISE_SHORTCUTS;
  }
};
customElements.define(MostVisitedElement.is, MostVisitedElement);
function getCss20() {
  return [getCss3(), i(["/* Copyright 2025 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #import=//resources/cr_elements/cr_hidden_style_lit.css.js\n * #include=cr-hidden-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  --cr-searchbox-icon-border-radius: 8px;\n  align-items: center;\n  display: flex;\n  flex-shrink: 0;\n  justify-content: center;\n  width: var(--cr-searchbox-icon-container-size, 32px);\n}\n\n:host(:not([is-lens-searchbox_])) {\n  --cr-searchbox-icon-border-radius: 4px;\n}\n\n#container {\n  align-items: center;\n  aspect-ratio: 1 / 1;\n  border-radius: var(--cr-searchbox-icon-border-radius);\n  display: flex;\n  justify-content: center;\n  overflow: hidden;\n  position: relative;\n  width: 100%;\n}\n\n/* Entities may feature a dominant color background until image loads. */\n:host([has-image_]:not([in-searchbox])) #container {\n  background-color: var(--color-searchbox-results-icon-container-background,\n      var(--container-bg-color));\n}\n\n/* If icon is for a pedal or AiS, and it is not in the search box, apply background. */\n:host([has-icon-container-background]:not([in-searchbox])) #container {\n  background-color: var(--color-searchbox-answer-icon-background);\n}\n\n\n#image {\n  display: none;\n  height: 100%;\n  object-fit: contain;\n  width: 100%;\n}\n\n:host([has-image_]:not([in-searchbox])) #image {\n  display: initial;\n}\n\n\n#icon {\n  -webkit-mask-position: center;\n  -webkit-mask-repeat: no-repeat;\n  -webkit-mask-size: var(--cr-searchbox-results-search-icon-size, 16px);\n  background-color: var(--color-searchbox-search-icon-background);\n  height: 24px;\n  width: 24px;\n}\n\n#faviconImageContainer {\n  width: 24px;\n  height: 24px;\n\n  display: flex;\n  justify-content: center;\n  align-items: center;\n}\n\n#faviconImage {\n  height: 16px;\n  width: 16px;\n}\n\n@media (forced-colors: active) {\n  #icon {\n    --color-searchbox-search-icon-background: ButtonText;\n  }\n}\n\n:host([in-searchbox][is-lens-searchbox_]) #icon {\n  background-color: var(--color-searchbox-google-g-background);\n  height: var(--cr-searchbox-icon-size-in-searchbox);\n  width: var(--cr-searchbox-icon-size-in-searchbox);\n}\n\n:host([in-searchbox][favicon-image_*='//resources/cr_components/omnibox/icons/google_g.svg']) #faviconImage {\n  width: 24px;\n  height: 24px;\n}\n\n:host([in-searchbox]) #icon {\n  -webkit-mask-size: var(--cr-searchbox-icon-size-in-searchbox);\n}\n\n:host([in-searchbox]) #faviconImage {\n  width: var(--cr-searchbox-icon-size-in-searchbox);\n  height: var(--cr-searchbox-icon-size-in-searchbox);\n}\n\n:host([has-icon-container-background]:not([in-searchbox])) #icon {\n  background-color: var(--color-searchbox-answer-icon-foreground);\n}\n\n:host([has-icon-container-background][is-starter-pack]:not([in-searchbox])) #icon,\n:host([has-icon-container-background][is-featured-enterprise-search]:not([in-searchbox])) #icon {\n  background-color: var(--color-searchbox-results-starter-pack-icon,\n      var(--color-searchbox-answer-icon-foreground));\n}\n\n#iconImg {\n  height: var(--cr-searchbox-results-search-icon-size, 16px);\n  width: var(--cr-searchbox-results-search-icon-size, 16px);\n}\n\n:host([in-searchbox]) #iconImg {\n  height: var(--cr-searchbox-icon-size-in-searchbox,\n      var(--cr-searchbox-results-search-icon-size));\n  width: var(--cr-searchbox-icon-size-in-searchbox,\n      var(--cr-searchbox-results-search-icon-size));\n}\n\n:host([has-image_]:not([in-searchbox])) #icon,\n:host([has-image_]:not([in-searchbox])) #iconImg,\n:host([has-image_]:not([in-searchbox])) #faviconImageContainer {\n  display: none;\n}\n\n:host(:not([in-searchbox])[is-lens-searchbox_]) #container {\n  background-color: var(--color-searchbox-results-icon-container-background);\n  border-radius: 4000px;\n}\n"])];
}
function getHtml13() {
  return b2`<!--_html_template_start_-->
<div id="container"
    style="--container-bg-color:${this.getContainerBgColor_()};">
  <img id="image" src="${this.imageSrc_}" ?hidden="${!this.showImage_}"
      @load="${this.onImageLoad_}" @error="${this.onImageError_}">

  <div ?hidden="${this.showIconImg_}">
    <div id="icon" style="-webkit-mask-image: ${this.maskImage};"
        ?hidden="${this.showFaviconImage_}">
    </div>
    <div id="faviconImageContainer"
        ?hidden="${!this.showFaviconImage_}">
      <img id="faviconImage" src="${this.faviconImage_}"
          srcset="${this.faviconImageSrcSet_}"
          @load="${this.onFaviconLoad_}"
          @error="${this.onFaviconError_}">
    </div>
  </div>

  <img id="iconImg" src="${this.iconSrc_}" ?hidden="${!this.showIconImg_}"
      @load="${this.onIconLoad_}">
</div>
<!--_html_template_end_-->`;
}
var CALCULATOR = "search-calculator-answer";
var DOCUMENT_MATCH_TYPE = "document";
var FEATURED_ENTERPRISE_SEARCH = "featured-enterprise-search";
var HISTORY_CLUSTER_MATCH_TYPE = "history-cluster";
var PEDAL = "pedal";
var STARTER_PACK = "starter-pack";
var SearchboxIconElement = class extends CrLitElement {
  static get is() {
    return "cr-searchbox-icon";
  }
  static get styles() {
    return getCss20();
  }
  render() {
    return getHtml13.bind(this)();
  }
  static get properties() {
    return {
      //========================================================================
      // Public properties
      //========================================================================
      /**
       * The default icon to show when no match is selected and/or for
       * non-navigation matches. Only set in the context of the searchbox input.
       */
      defaultIcon: { type: String },
      /**  Whether icon should have a background. */
      hasIconContainerBackground: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether icon is in searchbox or not. Used to prevent
       * the match icon of rich suggestions from showing in the context of the
       * searchbox input.
       */
      inSearchbox: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether icon is in keyword mode. Used in searchbox input to show
       * the generic search loupe icon instead of match favicons when the
       * keyword does not have its own custom icon.
       */
      inKeywordMode: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether icon belongs to an answer or not. Used to prevent
       * the match image from taking size of container.
       */
      /**
       * Whether icon belongs to a starter pack match.
       */
      isStarterPack: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether icon belongs to a featured enterprise search match.
       */
      isFeaturedEnterpriseSearch: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether suggestion answer is of answer type weather. Weather answers
       * don't have the same background as other suggestion answers.
       */
      /**
       * Whether suggestion is an enterprise search aggregator people
       * suggestion. Enterprise search aggregator people suggestions should not
       * use a set background color when image is missing, unlike other rich
       * suggestion answers.
       */
      isEnterpriseSearchAggregatorPeopleType: {
        type: Boolean,
        reflect: true
      },
      /** Used as a mask image on #icon if `faviconImage_` is empty. */
      maskImage: {
        type: String,
        reflect: true
      },
      /**
       * The URL of the current webpage when focused in the searchbox before
       * typing, used to load the page's favicon. Empty when typing, on the NTP,
       * or for consumers that do not provide a page URL.
       */
      pageUrl: { type: String },
      match: { type: Object },
      //========================================================================
      // Private properties
      //========================================================================
      /** Used as the image src for the #faviconImage if non-empty. */
      faviconImage_: {
        type: String,
        reflect: true
      },
      /**
       * Used as the image srcset for the #faviconImage if non-empty.
       */
      faviconImageSrcSet_: { state: true, type: String },
      /**
       * Whether the match features an image (as opposed to an icon or favicon).
       */
      hasImage_: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether to use the favicon image instead of the default vector icon
       * for the suggestion.
       */
      showFaviconImage_: { state: true, type: Boolean },
      /**
       * Flag indicating whether or not a favicon is loading.
       */
      faviconLoading_: { state: true, type: Boolean },
      /**
       * Flag indicating whether or not a favicon was successfully loaded.
       * This is used to force the WebUI popup to make use of the default vector
       * icon when the favicon image is unavailable.
       */
      faviconError_: { state: true, type: Boolean },
      /** Used as the image src for the #iconImg if non-empty. */
      iconSrc_: { state: true, type: String },
      /**
       * Flag indicating whether or not an icon image is loading. This is used
       * to show a default icon while the image is loading.
       */
      iconLoading_: { state: true, type: Boolean },
      /**
       * Whether to use the icon image instead of the default icon for the
       * suggestion.
       */
      showIconImg_: { state: true, type: Boolean },
      showImage_: { state: true, type: Boolean },
      imageSrc_: { state: true, type: String },
      /**
       * Flag indicating whether or not an image is loading. This is used to
       * show a placeholder color while the image is loading.
       */
      imageLoading_: { state: true, type: Boolean },
      /**
       * Flag indicating whether or not an image was successfully loaded. This
       * is used to suppress the default "broken image" icon as needed.
       */
      imageError_: {
        state: true,
        type: Boolean
      },
      isTopChromeSearchbox_: { state: true, type: Boolean },
      isLensSearchbox_: {
        type: Boolean,
        reflect: true
      }
    };
  }
  #defaultIcon = "";
  get defaultIcon() {
    return this.#defaultIcon;
  }
  set defaultIcon(_2) {
    this.#defaultIcon = _2;
  }
  #hasIconContainerBackground = false;
  get hasIconContainerBackground() {
    return this.#hasIconContainerBackground;
  }
  set hasIconContainerBackground(_2) {
    this.#hasIconContainerBackground = _2;
  }
  #inKeywordMode = false;
  get inKeywordMode() {
    return this.#inKeywordMode;
  }
  set inKeywordMode(_2) {
    this.#inKeywordMode = _2;
  }
  #inSearchbox = false;
  get inSearchbox() {
    return this.#inSearchbox;
  }
  set inSearchbox(_2) {
    this.#inSearchbox = _2;
  }
  #isStarterPack = false;
  get isStarterPack() {
    return this.#isStarterPack;
  }
  set isStarterPack(_2) {
    this.#isStarterPack = _2;
  }
  #isFeaturedEnterpriseSearch = false;
  get isFeaturedEnterpriseSearch() {
    return this.#isFeaturedEnterpriseSearch;
  }
  set isFeaturedEnterpriseSearch(_2) {
    this.#isFeaturedEnterpriseSearch = _2;
  }
  #isEnterpriseSearchAggregatorPeopleType = false;
  get isEnterpriseSearchAggregatorPeopleType() {
    return this.#isEnterpriseSearchAggregatorPeopleType;
  }
  set isEnterpriseSearchAggregatorPeopleType(_2) {
    this.#isEnterpriseSearchAggregatorPeopleType = _2;
  }
  #maskImage = "";
  get maskImage() {
    return this.#maskImage;
  }
  set maskImage(_2) {
    this.#maskImage = _2;
  }
  #match = null;
  get match() {
    return this.#match;
  }
  set match(_2) {
    this.#match = _2;
  }
  #pageUrl = "";
  get pageUrl() {
    return this.#pageUrl;
  }
  set pageUrl(_2) {
    this.#pageUrl = _2;
  }
  #faviconImage_ = "";
  get faviconImage_() {
    return this.#faviconImage_;
  }
  set faviconImage_(_2) {
    this.#faviconImage_ = _2;
  }
  #faviconImageSrcSet_ = "";
  get faviconImageSrcSet_() {
    return this.#faviconImageSrcSet_;
  }
  set faviconImageSrcSet_(_2) {
    this.#faviconImageSrcSet_ = _2;
  }
  #hasImage_ = false;
  get hasImage_() {
    return this.#hasImage_;
  }
  set hasImage_(_2) {
    this.#hasImage_ = _2;
  }
  #showFaviconImage_ = false;
  get showFaviconImage_() {
    return this.#showFaviconImage_;
  }
  set showFaviconImage_(_2) {
    this.#showFaviconImage_ = _2;
  }
  #faviconLoading_ = false;
  get faviconLoading_() {
    return this.#faviconLoading_;
  }
  set faviconLoading_(_2) {
    this.#faviconLoading_ = _2;
  }
  #faviconError_ = false;
  get faviconError_() {
    return this.#faviconError_;
  }
  set faviconError_(_2) {
    this.#faviconError_ = _2;
  }
  #iconSrc_ = "";
  get iconSrc_() {
    return this.#iconSrc_;
  }
  set iconSrc_(_2) {
    this.#iconSrc_ = _2;
  }
  #iconLoading_ = false;
  get iconLoading_() {
    return this.#iconLoading_;
  }
  set iconLoading_(_2) {
    this.#iconLoading_ = _2;
  }
  #showIconImg_ = false;
  get showIconImg_() {
    return this.#showIconImg_;
  }
  set showIconImg_(_2) {
    this.#showIconImg_ = _2;
  }
  #showImage_ = false;
  get showImage_() {
    return this.#showImage_;
  }
  set showImage_(_2) {
    this.#showImage_ = _2;
  }
  #imageSrc_ = "";
  get imageSrc_() {
    return this.#imageSrc_;
  }
  set imageSrc_(_2) {
    this.#imageSrc_ = _2;
  }
  #imageLoading_ = false;
  get imageLoading_() {
    return this.#imageLoading_;
  }
  set imageLoading_(_2) {
    this.#imageLoading_ = _2;
  }
  #imageError_ = false;
  get imageError_() {
    return this.#imageError_;
  }
  set imageError_(_2) {
    this.#imageError_ = _2;
  }
  #isTopChromeSearchbox_ = loadTimeData.getBoolean("isTopChromeSearchbox");
  get isTopChromeSearchbox_() {
    return this.#isTopChromeSearchbox_;
  }
  set isTopChromeSearchbox_(_2) {
    this.#isTopChromeSearchbox_ = _2;
  }
  #isLensSearchbox_ = loadTimeData.getBoolean("isLensSearchbox");
  get isLensSearchbox_() {
    return this.#isLensSearchbox_;
  }
  set isLensSearchbox_(_2) {
    this.#isLensSearchbox_ = _2;
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    const changedPrivateProperties = changedProperties;
    if (changedProperties.has("match")) {
      const oldIconSrc = this.iconSrc_;
      this.iconSrc_ = this.computeIconSrc_();
      if (this.iconSrc_ !== oldIconSrc) {
        this.iconLoading_ = !!this.iconSrc_;
      }
      const oldImageSrc = this.imageSrc_;
      this.imageSrc_ = this.computeImageSrc_();
      if (this.imageSrc_ !== oldImageSrc) {
        this.imageLoading_ = !!this.imageSrc_;
        this.imageError_ = false;
      }
      this.isEnterpriseSearchAggregatorPeopleType = this.computeIsEnterpriseSearchAggregatorPeopleType_();
      this.isStarterPack = this.computeIsStarterPack_();
      this.isFeaturedEnterpriseSearch = this.computeIsFeaturedEnterpriseSearch_();
      this.hasImage_ = this.computeHasImage_();
      this.hasIconContainerBackground = this.computeHasIconContainerBackground_();
    }
    if (changedProperties.has("match") || changedProperties.has("pageUrl") || changedProperties.has("defaultIcon") || changedProperties.has("inKeywordMode")) {
      this.maskImage = this.computeMaskImage_();
    }
    if (changedProperties.has("match") || changedProperties.has("pageUrl") || changedProperties.has("defaultIcon") || changedPrivateProperties.has("isTopChromeSearchbox_")) {
      const oldFaviconImage = this.faviconImage_;
      this.faviconImage_ = this.computeFaviconImage_();
      if (this.faviconImage_ !== oldFaviconImage) {
        this.faviconLoading_ = !!this.faviconImage_;
        this.faviconError_ = false;
      }
      this.faviconImageSrcSet_ = this.computeFaviconImageSrcSet_();
    }
    if (changedProperties.has("match") || changedProperties.has("pageUrl") || changedProperties.has("defaultIcon") || changedProperties.has("inKeywordMode") || changedPrivateProperties.has("isLensSearchbox_") || changedPrivateProperties.has("isTopChromeSearchbox_") || changedPrivateProperties.has("faviconImage_") || changedPrivateProperties.has("faviconLoading_") || changedPrivateProperties.has("faviconError_")) {
      this.showFaviconImage_ = this.computeShowFaviconImage_();
    }
    if (changedProperties.has("match") || changedPrivateProperties.has("imageSrc_") || changedPrivateProperties.has("imageError_")) {
      this.showImage_ = this.computeShowImage_();
    }
    if (changedProperties.has("match") || changedPrivateProperties.has("isLensSearchbox_") || changedPrivateProperties.has("iconLoading_")) {
      this.showIconImg_ = this.computeShowIconImg_();
    }
  }
  //============================================================================
  // Helpers
  //============================================================================
  computeFaviconUrl_(scaleFactor) {
    const url = this.match?.destinationUrl || this.pageUrl;
    if (!url) {
      return "";
    }
    return getFaviconUrl(
      /* url= */
      url,
      {
        forceLightMode: !this.isTopChromeSearchbox_,
        forceEmptyDefaultFavicon: true,
        scaleFactor: `${scaleFactor}x`
      }
    );
  }
  computeFaviconImageSrcSet_() {
    if (!this.faviconImage_.startsWith("chrome://favicon2/")) {
      return "";
    }
    const url2x = new URL(this.faviconImage_);
    url2x.searchParams.set("scaleFactor", "2x");
    return `${this.faviconImage_} 1x, ${url2x.toString()} 2x`;
  }
  computeFaviconImage_() {
    if (this.match && !this.match.isSearchType) {
      if (this.match.type === DOCUMENT_MATCH_TYPE || this.match.type === PEDAL || this.match.isEnterpriseSearchAggregatorPeopleType) {
        return this.match.iconPath;
      }
      if (this.match.type !== HISTORY_CLUSTER_MATCH_TYPE && this.match.type !== FEATURED_ENTERPRISE_SEARCH) {
        return this.computeFaviconUrl_(
          /* scaleFactor= */
          1
        );
      }
    }
    if (this.inSearchbox && this.isTopChromeSearchbox_ && this.pageUrl && !this.match) {
      return this.computeFaviconUrl_(
        /* scaleFactor= */
        1
      );
    }
    if (this.defaultIcon === "//resources/cr_components/searchbox/icons/google_g.svg" || this.defaultIcon === "//resources/cr_components/searchbox/icons/google_g_gradient.svg" || this.inSearchbox && this.isTopChromeSearchbox_ && this.defaultIcon.startsWith("chrome://favicon2/")) {
      return this.defaultIcon;
    }
    return "";
  }
  computeHasImage_() {
    return !!this.match && !!this.match.imageUrl;
  }
  computeIsEnterpriseSearchAggregatorPeopleType_() {
    return this.match?.isEnterpriseSearchAggregatorPeopleType || false;
  }
  computeShowIconImg_() {
    return !this.isLensSearchbox_ && !!this.match && !!this.match.iconUrl && !this.iconLoading_;
  }
  computeMaskImage_() {
    if (this.inSearchbox && this.inKeywordMode) {
      return "url(//resources/cr_components/searchbox/icons/search_cr23.svg)";
    }
    if (this.isLensSearchbox_ && this.inSearchbox) {
      return `url(${this.defaultIcon})`;
    }
    if (this.match && (!this.match.isTwoRowSuggestion || this.match.type === STARTER_PACK || this.match.type === FEATURED_ENTERPRISE_SEARCH || this.match.isEnterpriseSearchAggregatorPeopleType || this.isTopChromeSearchbox_ || !this.inSearchbox)) {
      return `url(${this.match.iconPath})`;
    }
    if (this.inSearchbox && this.isTopChromeSearchbox_) {
      if (this.pageUrl && !this.match) {
        return "url(//resources/cr_components/searchbox/icons/page_cr23.svg)";
      }
      if (this.defaultIcon.startsWith("chrome://favicon2/")) {
        return "url(//resources/cr_components/searchbox/icons/search_cr23.svg)";
      }
    }
    return `url(${this.defaultIcon})`;
  }
  // Controls whether the favicon image should be rendered instead of the mask
  // image.
  computeShowFaviconImage_() {
    if (this.inSearchbox && this.inKeywordMode) {
      return false;
    }
    if (!this.faviconImage_ || this.faviconLoading_ || this.faviconError_) {
      return false;
    }
    if (this.match && !this.match.isSearchType) {
      if (this.isLensSearchbox_ || this.match.type === STARTER_PACK || this.match.type === PEDAL) {
      } else {
        return true;
      }
    }
    if ((!this.match || this.match.isSearchType) && this.inSearchbox && this.isTopChromeSearchbox_ && (this.faviconImage_.startsWith("chrome://favicon2/") || this.faviconImage_ === "//resources/cr_components/searchbox/icons/google_g.svg" || this.faviconImage_ === "//resources/cr_components/searchbox/icons/google_g_gradient.svg")) {
      return true;
    }
    const themedIcons = [
      "calendar",
      "drive_docs",
      "drive_folder",
      "drive_form",
      "drive_image",
      "drive_logo",
      "drive_pdf",
      "drive_sheets",
      "drive_slides",
      "drive_video",
      "google_agentspace_logo",
      "google_agentspace_logo_25",
      "google_g",
      "google_g_gradient",
      "note",
      "sites"
    ];
    for (const icon of themedIcons) {
      if (this.faviconImage_ === "//resources/cr_components/searchbox/icons/" + icon + ".svg") {
        return true;
      }
    }
    return false;
  }
  computeSrc_(url) {
    if (!url) {
      return "";
    }
    if (url.startsWith("data:image/")) {
      return url;
    }
    return `//image?staticEncode=true&encodeType=webp&url=${encodeURIComponent(url)}`;
  }
  computeIconSrc_() {
    return this.computeSrc_(this.match?.iconUrl);
  }
  computeShowImage_() {
    return !!this.imageSrc_ && !this.imageError_;
  }
  computeImageSrc_() {
    return this.computeSrc_(this.match?.imageUrl);
  }
  getContainerBgColor_() {
    return (this.imageLoading_ || this.imageError_) && this.match?.imageDominantColor ? (
      // .25 opacity matching c/b/u/views/omnibox/omnibox_match_cell_view.cc.
      this.match.imageDominantColor ? `${this.match.imageDominantColor}40` : "var(--cr-searchbox-match-icon-container-background-fallback)"
    ) : "transparent";
  }
  onFaviconLoad_() {
    this.faviconLoading_ = false;
    this.faviconError_ = false;
  }
  onFaviconError_() {
    this.faviconLoading_ = false;
    this.faviconError_ = true;
  }
  onIconLoad_() {
    this.iconLoading_ = false;
  }
  onImageLoad_() {
    this.imageLoading_ = false;
    this.imageError_ = false;
  }
  onImageError_() {
    this.imageLoading_ = false;
    this.imageError_ = true;
  }
  // All pedals, starter pack/featured enterprise search suggestions, and AiS
  // except weather should have a colored background container that matches the
  // current theme.
  // TODO(niharm): Refactor logic in C++ and send via mojom in
  // "chrome/browser/ui/webui/searchbox/searchbox_handler.cc".
  computeHasIconContainerBackground_() {
    if (this.match) {
      return this.match.type === PEDAL || this.match.type === HISTORY_CLUSTER_MATCH_TYPE || this.match.type === CALCULATOR || this.match.type === STARTER_PACK || this.match.type === FEATURED_ENTERPRISE_SEARCH;
    }
    return false;
  }
  computeIsStarterPack_() {
    return this.match?.type === STARTER_PACK;
  }
  computeIsFeaturedEnterpriseSearch_() {
    return this.match?.type === FEATURED_ENTERPRISE_SEARCH;
  }
};
customElements.define(SearchboxIconElement.is, SearchboxIconElement);
function getCss21() {
  return [getCss7(), i(["/* Copyright 2026 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=//resources/cr_elements/cr_icons_lit.css.js\n * #scheme=relative\n * #include=cr-icons-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  display: flex;\n  flex: 1;\n  width: 100%;\n}\n\n#inputInnerContainer {\n  align-items: flex-start;\n  display: flex;\n  min-height: var(--cr-searchbox-input-height, 48px);\n  position: relative;\n  width: 100%;\n}\n\n:host([multi-line-enabled]) textarea {\n  background-color: var(--color-cr-searchbox-input-textarea, transparent);\n  box-sizing: border-box;\n  field-sizing: content;\n  line-height: 24px;\n  max-height: 190px;\n  overflow-y: auto;\n  overflow-x: hidden;\n  padding-top: calc((var(--cr-searchbox-height) - 24px) / 2);\n  resize: none;\n  scrollbar-width: none;\n  white-space: pre-wrap;\n}\n\n:host([multi-line-enabled]) #input::-webkit-scrollbar {\n  display: none;\n}\n\n:is(input, textarea) {\n  background-color: var(--color-cr-searchbox-input-textarea, transparent);\n  border: none;\n  color: var(--color-searchbox-foreground);\n  font-family: inherit;\n  font-size: inherit;\n  height: 100%;\n  outline: none;\n  padding-top: 20px;\n  position: relative;\n  width: 100%;\n}\n\ninput::-webkit-search-decoration,\ninput::-webkit-search-results-button,\ninput::-webkit-search-results-decoration {\n  display: none;\n}\n\n/* Visually hide the cancel button but do not set display to none or\n * visibility to hidden as this causes issues with NVDA not reading out the\n * full value of the searchbox input as the user selects suggestion matches.\n * See crbug.com/1312442 for more context. */\ninput::-webkit-search-cancel-button {\n  appearance: none;\n  margin: 0;\n}\n\n:is(input, textarea)::placeholder {\n  color: var(--color-searchbox-placeholder);\n  opacity: var(--placeholder-opacity);\n  white-space: nowrap;\n}\n\n:is(input, textarea):focus::placeholder {\n  /* Visually hide the placeholder on focus. The placeholder will still be\n   * read by screen readers. Using color: transparent or other ways of\n   * visually hiding the placeholder does not work well with 'Find in page...'\n   * as the placeholder text can get highlighted. */\n  visibility: hidden;\n}\n\ncr-searchbox-icon {\n  height: 100%;\n  padding-inline-end: var(--cr-searchbox-icon-right-position, 4px);\n  padding-inline-start: var(--cr-searchbox-icon-left-position);\n  position: relative;\n  top: var(--cr-searchbox-icon-top-position);\n  pointer-events: none;\n}\n\n@media (forced-colors: active) {\n  cr-searchbox-icon {\n    border-radius: 4px;\n  }\n}\n\n:is(input, textarea),\ncr-searchbox-icon,\n#keyword {\n  z-index: 100;\n}\n\n#keyword {\n  align-self: center;\n  border-inline-end: 1px solid var(--color-omnibox-keyword-selected);\n  color: var(--color-omnibox-keyword-selected);\n  margin-inline-end: 8px;\n  padding-inline-end: 8px;\n  user-select: none;\n  white-space: nowrap;\n}\n\n.truncate {\n  overflow: hidden;\n  text-overflow: ellipsis;\n}\n\n\n"])];
}
function getHtml14() {
  return b2`<!--_html_template_start_-->
<div id="inputInnerContainer" part="input-inner-container">
    <slot name="contextual-entrypoint"></slot>
    <cr-searchbox-icon id="icon" .match="${this.selectedMatch}"
        page-url="${this.pageUrl}"
        default-icon="${this.searchboxIcon}"
        ?in-keyword-mode="${this.inKeywordMode_()}"
        in-searchbox part="icon">
    </cr-searchbox-icon>
    <slot name="thumbnail"></slot>
    ${this.inKeywordMode_() ? b2`<span id="keyword">${this.inputKeywordModel.displayText}</span>` : ""}
    ${this.multiLineEnabled ? b2`
      <textarea id="input" autocomplete="off"
          part="searchbox-input"
          spellcheck="false" aria-live="${this.inputAriaLive}" role="combobox"
          aria-expanded="${this.dropdownIsVisible}" aria-controls="matches"
          aria-description="${this.searchboxAriaDescription}"
          placeholder="${this.computePlaceholderText_()}"
          @copy="${this.onInputCopy_}"
          @cut="${this.onInputCut_}"
          @input="${this.onInputInput_}"
          @keydown="${this.onInputKeydown_}"
          @keyup="${this.onInputKeyup_}"
          @mousedown="${this.onInputMousedown_}"
          @paste="${this.onInputPaste_}"></textarea>
    ` : b2`
      <input id="input" class="truncate" type="search" autocomplete="off"
          part="searchbox-input"
          spellcheck="false" aria-live="${this.inputAriaLive}" role="combobox"
          aria-expanded="${this.dropdownIsVisible}" aria-controls="matches"
          aria-description="${this.searchboxAriaDescription}"
          placeholder="${this.computePlaceholderText_()}"
          @copy="${this.onInputCopy_}"
          @cut="${this.onInputCut_}"
          @input="${this.onInputInput_}"
          @keydown="${this.onInputKeydown_}"
          @keyup="${this.onInputKeyup_}"
          @mousedown="${this.onInputMousedown_}"
          @paste="${this.onInputPaste_}">
    `}
    <slot name="action-buttons"></slot>
    <slot name="compose-button"></slot>
</div>
<!--_html_template_end_-->`;
}
function getCss22() {
  return [i(["/* Copyright 2026 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n:host {\n  clip: rect(0 0 0 0);\n  height: 1px;\n  overflow: hidden;\n  position: fixed;\n  width: 1px;\n}\n"])];
}
function getHtml15() {
  return b2`
<div id="messages" role="alert" aria-live="polite" aria-relevant="additions">
</div>`;
}
var TIMEOUT_MS = 150;
var instances = /* @__PURE__ */ new Map();
function getInstance(container = document.body) {
  if (instances.has(container)) {
    return instances.get(container);
  }
  assert(container.isConnected);
  const instance2 = new CrA11yAnnouncerElement();
  container.appendChild(instance2);
  instances.set(container, instance2);
  return instance2;
}
var CrA11yAnnouncerElement = class extends CrLitElement {
  static get is() {
    return "cr-a11y-announcer";
  }
  static get styles() {
    return getCss22();
  }
  render() {
    return getHtml15.bind(this)();
  }
  currentTimeout_ = null;
  messages_ = [];
  disconnectedCallback() {
    super.disconnectedCallback();
    if (this.currentTimeout_ !== null) {
      clearTimeout(this.currentTimeout_);
      this.currentTimeout_ = null;
    }
    for (const [parent, instance2] of instances) {
      if (instance2 === this) {
        instances.delete(parent);
        break;
      }
    }
  }
  announce(message, timeout = TIMEOUT_MS) {
    if (this.currentTimeout_ !== null) {
      clearTimeout(this.currentTimeout_);
      this.currentTimeout_ = null;
    }
    this.messages_.push(message);
    this.currentTimeout_ = setTimeout(() => {
      const messagesDiv = this.shadowRoot.querySelector("#messages");
      messagesDiv.innerHTML = window.trustedTypes.emptyHTML;
      for (const message2 of this.messages_) {
        const div2 = document.createElement("div");
        div2.textContent = message2;
        messagesDiv.appendChild(div2);
      }
      this.dispatchEvent(new CustomEvent(
        "cr-a11y-announcer-messages-sent",
        { bubbles: true, detail: { messages: this.messages_.slice() } }
      ));
      this.messages_.length = 0;
      this.currentTimeout_ = null;
    }, timeout);
  }
};
customElements.define(CrA11yAnnouncerElement.is, CrA11yAnnouncerElement);
function mojoTimeTicks(timeTicks) {
  return { internalValue: BigInt(Math.floor(timeTicks * 1e3)) };
}
function sideTypeToClass(sideType) {
  switch (sideType) {
    case SideType.kDefaultPrimary:
      return "primary-side";
    case SideType.kSecondary:
      return "secondary-side";
    default:
      assertNotReached("Unexpected side type");
  }
}
function renderTypeToClass(renderType) {
  switch (renderType) {
    case RenderType.kDefaultVertical:
      return "vertical";
    case RenderType.kHorizontal:
      return "horizontal";
    case RenderType.kGrid:
      return "grid";
    default:
      assertNotReached("Unexpected render type");
  }
}
function markOnce(name) {
  if (!performance.getEntriesByName(name).length) {
    performance.mark(name);
    return true;
  }
  return false;
}
function afterNextPaint(callback) {
  requestAnimationFrame(() => {
    setTimeout(callback, 0);
  });
}
function announce(element, message) {
  if (!message) {
    return;
  }
  if (element.ariaNotify) {
    element.ariaNotify(message, { priority: "high" });
  } else {
    getInstance(element).announce(message);
  }
}
CSS.registerProperty({
  name: "--placeholder-opacity",
  syntax: "<number>",
  initialValue: "1",
  inherits: true
});
var MULTILINE_INPUT_HEIGHT_THRESHOLD = 48;
var SearchboxInputElementBase = I18nMixinLit(CrLitElement);
var SearchboxInputElement = class extends SearchboxInputElementBase {
  static get is() {
    return "cr-searchbox-input";
  }
  static get styles() {
    return getCss21();
  }
  render() {
    return getHtml14.bind(this)();
  }
  static get properties() {
    return {
      dropdownIsVisible: { type: Boolean, reflect: true },
      inputAriaLive: { type: String },
      multiLineEnabled: { type: Boolean, reflect: true },
      placeholderText: { type: String },
      searchboxAriaDescription: { type: String },
      searchboxIcon: { type: String },
      selectedMatch: { type: Object },
      /**
       * The URL of the current webpage when focused in the searchbox before
       * typing, used to load the page's favicon. Empty when typing, on the NTP,
       * or for consumers that do not provide a page URL.
       */
      pageUrl: { type: String },
      inputKeywordModel: { type: Object },
      inputHasMatches: { type: Boolean },
      allowFilePaste: { type: Boolean }
    };
  }
  #dropdownIsVisible = false;
  get dropdownIsVisible() {
    return this.#dropdownIsVisible;
  }
  set dropdownIsVisible(_2) {
    this.#dropdownIsVisible = _2;
  }
  #inputAriaLive = "";
  get inputAriaLive() {
    return this.#inputAriaLive;
  }
  set inputAriaLive(_2) {
    this.#inputAriaLive = _2;
  }
  #multiLineEnabled = false;
  get multiLineEnabled() {
    return this.#multiLineEnabled;
  }
  set multiLineEnabled(_2) {
    this.#multiLineEnabled = _2;
  }
  #placeholderText = void 0;
  get placeholderText() {
    return this.#placeholderText;
  }
  set placeholderText(_2) {
    this.#placeholderText = _2;
  }
  #searchboxAriaDescription = "";
  get searchboxAriaDescription() {
    return this.#searchboxAriaDescription;
  }
  set searchboxAriaDescription(_2) {
    this.#searchboxAriaDescription = _2;
  }
  #searchboxIcon = "";
  get searchboxIcon() {
    return this.#searchboxIcon;
  }
  set searchboxIcon(_2) {
    this.#searchboxIcon = _2;
  }
  #selectedMatch = null;
  get selectedMatch() {
    return this.#selectedMatch;
  }
  set selectedMatch(_2) {
    this.#selectedMatch = _2;
  }
  #pageUrl = "";
  get pageUrl() {
    return this.#pageUrl;
  }
  set pageUrl(_2) {
    this.#pageUrl = _2;
  }
  #inputKeywordModel = null;
  get inputKeywordModel() {
    return this.#inputKeywordModel;
  }
  set inputKeywordModel(_2) {
    this.#inputKeywordModel = _2;
  }
  #inputHasMatches = false;
  get inputHasMatches() {
    return this.#inputHasMatches;
  }
  set inputHasMatches(_2) {
    this.#inputHasMatches = _2;
  }
  #allowFilePaste = false;
  get allowFilePaste() {
    return this.#allowFilePaste;
  }
  set allowFilePaste(_2) {
    this.#allowFilePaste = _2;
  }
  callbackRouter_;
  inputTextChangedListenerId_ = null;
  lastInput_ = { text: "", inline: "" };
  isDeletingInput_ = false;
  pastedInInput_ = false;
  constructor() {
    super();
    this.callbackRouter_ = SearchboxBrowserProxy.getInstance().callbackRouter;
  }
  connectedCallback() {
    super.connectedCallback();
    this.inputTextChangedListenerId_ = this.callbackRouter_.setInputText.addListener(
      this.onSetInputText_.bind(this)
    );
  }
  disconnectedCallback() {
    super.disconnectedCallback();
    assert(this.inputTextChangedListenerId_);
    this.callbackRouter_.removeListener(this.inputTextChangedListenerId_);
  }
  get inputElement() {
    assert(this.$.input);
    return this.$.input;
  }
  focus() {
    assert(this.$.input);
    this.$.input.focus();
  }
  blur() {
    assert(this.$.input);
    this.$.input.blur();
  }
  select() {
    assert(this.$.input);
    this.$.input.select();
  }
  setSelectionRange(start, end, direction) {
    assert(this.$.input);
    this.$.input.setSelectionRange(start, end, direction);
  }
  getInputValue() {
    assert(this.$.input);
    return this.$.input.value;
  }
  setInputText(text) {
    markOnce("SearchboxInputElement::setInputText:StartupStart");
    this.onSetInputText_(text);
    if (markOnce("SearchboxInputElement::setInputText:StartupEnd")) {
      afterNextPaint(() => {
        markOnce("SearchboxInputElement::setInputText:StartupRendered");
      });
    }
  }
  setInput(update) {
    this.updateInput_(update);
  }
  lastInput() {
    return this.lastInput_;
  }
  isMultiline() {
    if (!this.$.input) {
      return false;
    }
    return this.multiLineEnabled && this.$.input.scrollHeight > MULTILINE_INPUT_HEIGHT_THRESHOLD;
  }
  preventInlineAutocomplete(input) {
    const caretNotAtEnd = this.$.input ? this.$.input.selectionStart !== input.length : false;
    return this.isDeletingInput_ || this.pastedInInput_ || caretNotAtEnd;
  }
  //============================================================================
  // Callbacks
  //============================================================================
  onSetInputText_(inputText) {
    this.updateInput_({ text: inputText, inline: "" });
  }
  //============================================================================
  // Event handlers
  //============================================================================
  onInputCopy_(e5) {
    this.onInputCutCopy_(e5);
  }
  onInputCut_(e5) {
    this.onInputCutCopy_(e5);
  }
  onInputCutCopy_(e5) {
    if (!this.$.input.value || this.$.input.selectionStart !== 0 || this.$.input.selectionEnd !== this.$.input.value.length || !this.inputHasMatches) {
      return;
    }
    if (this.selectedMatch && !this.selectedMatch.isSearchType) {
      e5.clipboardData.setData("text/plain", this.selectedMatch.destinationUrl);
      e5.preventDefault();
      if (e5.type === "cut") {
        this.updateInput_({ text: "", inline: "" });
        this.fire("searchbox-input-text-updated", {
          value: "",
          isComposing: false,
          event: e5
        });
      }
    }
  }
  onInputInput_(e5) {
    const inputValue = this.$.input.value;
    const lastInputValue = this.lastInput_.text + this.lastInput_.inline;
    if (lastInputValue === inputValue) {
      return;
    }
    this.updateInput_({ text: inputValue, inline: "" });
    if (inputValue.length > 0 && markOnce("SearchboxInputElement::onInputInput_:HasContent")) {
      afterNextPaint(() => {
        markOnce("SearchboxInputElement::onInputInput_:ContentRendered");
      });
    }
    this.fire("searchbox-input-text-updated", {
      value: inputValue,
      isComposing: e5.isComposing,
      event: e5
    });
    if (loadTimeData.getBoolean("reportMetrics")) {
      const charTyped = !this.isDeletingInput_ && !!inputValue.trim();
      const metricsReporter = MetricsReporterImpl.getInstance();
      if (charTyped) {
        if (!metricsReporter.hasLocalMark("CharTyped")) {
          metricsReporter.mark("CharTyped");
        }
      } else {
        metricsReporter.clearMark("CharTyped");
      }
    }
    this.pastedInInput_ = false;
  }
  onInputKeydown_(e5) {
    if (e5.isComposing || !this.lastInput_.inline) {
      return;
    }
    const inputValue = this.$.input.value;
    const inputSelection = inputValue.substring(
      this.$.input.selectionStart,
      this.$.input.selectionEnd
    );
    const lastInputValue = this.lastInput_.text + this.lastInput_.inline;
    if (inputSelection === this.lastInput_.inline && inputValue === lastInputValue && this.lastInput_.inline[0].toLocaleLowerCase() === e5.key.toLocaleLowerCase()) {
      const text = this.lastInput_.text + e5.key;
      assert(text);
      this.updateInput_({
        text,
        inline: this.lastInput_.inline.substr(1)
      });
      this.fire("searchbox-input-text-updated", {
        value: this.lastInput_.text,
        isComposing: false,
        event: e5
      });
      if (loadTimeData.getBoolean("reportMetrics")) {
        const metricsReporter = MetricsReporterImpl.getInstance();
        if (!metricsReporter.hasLocalMark("CharTyped")) {
          metricsReporter.mark("CharTyped");
        }
      }
      e5.preventDefault();
      e5.stopPropagation();
    }
  }
  onInputKeyup_(e5) {
    if (e5.key !== "Tab") {
      return;
    }
    this.fire(
      "input-focus-changed",
      { value: this.$.input.value, isOnFocus: !this.$.input.value }
    );
  }
  onInputMousedown_(e5) {
    if (e5 && e5.button !== 0) {
      return;
    }
    this.fire(
      "input-focus-changed",
      { value: this.$.input.value, isOnFocus: !this.$.input.value }
    );
  }
  onInputPaste_(e5) {
    this.fire("searchbox-input-pasted");
    if (this.allowFilePaste && e5.clipboardData?.files && e5.clipboardData.files.length > 0) {
      e5.preventDefault();
      this.fire("searchbox-input-files-pasted", {
        files: e5.clipboardData.files
      });
      return;
    }
    this.pastedInInput_ = true;
  }
  /**
   * Updates the input state (text and inline autocompletion) with |update|.
   */
  updateInput_(update) {
    const newInput = Object.assign({}, this.lastInput_, update);
    const newInputValue = newInput.text + newInput.inline;
    const lastInputValue = this.lastInput_.text + this.lastInput_.inline;
    const inlineDiffers = newInput.inline !== this.lastInput_.inline;
    const preserveSelection = !inlineDiffers && !update.moveCursorToEnd;
    let needsSelectionUpdate = !preserveSelection;
    const oldSelectionStart = this.$.input?.selectionStart || null;
    const oldSelectionEnd = this.$.input?.selectionEnd || null;
    if (this.$.input && newInputValue !== this.$.input.value) {
      this.$.input.value = newInputValue;
      needsSelectionUpdate = true;
    }
    if (this.$.input && newInputValue.trim() && needsSelectionUpdate) {
      this.$.input.selectionStart = preserveSelection ? oldSelectionStart : update.moveCursorToEnd ? newInputValue.length : newInput.text.length;
      this.$.input.selectionEnd = preserveSelection ? oldSelectionEnd : newInputValue.length;
    }
    this.isDeletingInput_ = update.isDeletingInput ?? (lastInputValue.length > newInputValue.length && lastInputValue.startsWith(newInputValue));
    this.lastInput_ = newInput;
  }
  computePlaceholderText_() {
    return this.placeholderText ?? this.i18n("searchBoxHint");
  }
  inKeywordMode_() {
    return this.inputKeywordModel?.type === KeywordType.kInKeyword;
  }
};
customElements.define(SearchboxInputElement.is, SearchboxInputElement);
function getCss23() {
  return [getCss10(), i(["/* Copyright 2025 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=//resources/cr_elements/cr_shared_style_lit.css.js\n * #scheme=relative\n * #include=cr-shared-style-lit\n * #css_wrapper_metadata_end */\n\n:host {\n  border: solid 1px var(--color-searchbox-results-action-chip);\n  border-radius: 8px;\n  display: flex;\n  height: var(--cr-searchbox-results-action-chip-height, 28px);\n  min-width: 0;\n  outline: none;\n  padding-inline-end: 8px;\n  padding-inline-start: 8px;\n  position: relative;\n  transition: background-color 0.25s;\n}\n\n:host(:hover) {\n  background-color: var(--color-searchbox-results-button-hover);\n}\n\n:host(:focus),\n:host(.selected) {\n  margin: 2px;\n  box-shadow: none;\n}\n\n:host(.selected:hover) {\n  background-color: var(--color-searchbox-results-button-selected-hover);\n}\n\n:host(:active) #overlay {\n  background-color:\n      var(--color-omnibox-results-button-ink-drop-selected-row-hovered);\n}\n\n:host(.selected:active) #overlay {\n  background-color:\n      var(--color-omnibox-results-button-ink-drop-selected-row-selected);\n}\n\n#overlay {\n  --overlay-inset: calc(var(--border-width) * -1);\n  border-radius: inherit;\n  display: inherit;\n  position: absolute;\n  top: var(--overlay-inset);\n  left: var(--overlay-inset);\n  right: var(--overlay-inset);\n  bottom: var(--overlay-inset);\n}\n\n.contents {\n  align-items: center;\n  display: flex;\n  min-width: 0;\n}\n\n#action-icon {\n  flex-shrink: 0;\n  -webkit-mask-position: center;\n  -webkit-mask-repeat: no-repeat;\n  -webkit-mask-size: 15px;\n  background-color: var(--color-searchbox-results-action-chip-icon);\n  background-position: center center;\n  background-repeat: no-repeat;\n  height: 16px;\n  width: 16px;\n}\n\n:host-context(:is(:focus, [selected])) #action-icon {\n  background-color: var(--color-searchbox-results-action-chip-icon-selected,\n      var(--color-searchbox-results-action-chip-icon));\n}\n\n#text {\n  overflow: hidden;\n  padding-inline-start: 8px;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n}\n"])];
}
function getHtml16() {
  return b2`<!--_html_template_start_-->
<div id="overlay"></div>
<div class="contents" title="${this.suggestionContents}">
  <div id="action-icon" style="${this.iconStyle_}"></div>
  <div id="text" .innerHTML="${this.hintHtml_}"></div>
</div>
<!--_html_template_end_-->`;
}
var SearchboxActionElement = class extends CrLitElement {
  static get is() {
    return "cr-searchbox-action";
  }
  static get styles() {
    return getCss23();
  }
  render() {
    return getHtml16.bind(this)();
  }
  static get properties() {
    return {
      hint: { type: String },
      hintHtml_: {
        state: true,
        type: String
      },
      suggestionContents: { type: String },
      iconPath: { type: String },
      iconStyle_: {
        state: true,
        type: String
      },
      ariaLabel: { type: String },
      // Index of the action in the autocomplete result. Used to inform handler
      // of action that was selected.
      actionIndex: { type: Number }
    };
  }
  #hint = "";
  get hint() {
    return this.#hint;
  }
  set hint(_2) {
    this.#hint = _2;
  }
  #hintHtml_ = window.trustedTypes.emptyHTML;
  get hintHtml_() {
    return this.#hintHtml_;
  }
  set hintHtml_(_2) {
    this.#hintHtml_ = _2;
  }
  #suggestionContents = "";
  get suggestionContents() {
    return this.#suggestionContents;
  }
  set suggestionContents(_2) {
    this.#suggestionContents = _2;
  }
  #iconPath = "";
  get iconPath() {
    return this.#iconPath;
  }
  set iconPath(_2) {
    this.#iconPath = _2;
  }
  #iconStyle_ = "";
  get iconStyle_() {
    return this.#iconStyle_;
  }
  set iconStyle_(_2) {
    this.#iconStyle_ = _2;
  }
  #ariaLabel = "";
  get ariaLabel() {
    return this.#ariaLabel;
  }
  set ariaLabel(_2) {
    this.#ariaLabel = _2;
  }
  #actionIndex = -1;
  get actionIndex() {
    return this.#actionIndex;
  }
  set actionIndex(_2) {
    this.#actionIndex = _2;
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("hint")) {
      this.hintHtml_ = this.computeHintHtml_();
    }
    if (changedProperties.has("iconPath")) {
      this.iconStyle_ = this.computeActionIconStyle_();
    }
  }
  firstUpdated() {
    this.addEventListener("click", (event) => this.onActionClick_(event));
    this.addEventListener("auxclick", (event) => this.onActionClick_(event));
    this.addEventListener("keydown", (event) => this.onActionKeyDown_(event));
    this.addEventListener(
      "mousedown",
      (event) => this.onActionMouseDown_(event)
    );
  }
  onActionClick_(e5) {
    this.fire("execute-action", {
      event: e5,
      actionIndex: this.actionIndex
    });
    e5.preventDefault();
    e5.stopPropagation();
  }
  onActionKeyDown_(e5) {
    if (e5.key && (e5.key === "Enter" || e5.key === " ")) {
      this.onActionClick_(e5);
    }
  }
  onActionMouseDown_(e5) {
    e5.preventDefault();
  }
  //============================================================================
  // Helpers
  //============================================================================
  computeHintHtml_() {
    if (this.hint) {
      return sanitizeInnerHtml(this.hint);
    }
    return window.trustedTypes.emptyHTML;
  }
  computeActionIconStyle_() {
    if (this.iconPath.startsWith("data:image/")) {
      return `background-image: url(${this.iconPath})`;
    }
    return `-webkit-mask-image: url(${this.iconPath})`;
  }
};
customElements.define(SearchboxActionElement.is, SearchboxActionElement);
function getCss24() {
  return [i(["/* Copyright 2022 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n.action-icon {\n  --cr-icon-button-active-background-color:\n      var(--color-new-tab-page-active-background);\n  --cr-icon-button-fill-color: var(--color-searchbox-results-icon);\n  --cr-icon-button-focus-outline-color:\n      var(--color-searchbox-results-icon-focused-outline);\n  --cr-icon-button-hover-background-color:\n      var(--color-searchbox-results-button-hover);\n  --cr-icon-button-icon-size: 16px;\n  --cr-icon-button-margin-end: 0;\n  --cr-icon-button-margin-start: 0;\n  --cr-icon-button-size: 24px;\n}\n"])];
}
function getCss25() {
  return [getCss3(), getCss7(), getCss24(), i([`/* Copyright 2025 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=//resources/cr_elements/cr_hidden_style_lit.css.js
 * #import=//resources/cr_elements/cr_icons_lit.css.js
 * #import=./searchbox_dropdown_shared_style.css.js
 * #scheme=relative
 * #include=cr-hidden-style-lit cr-icons-lit searchbox-dropdown-shared-style
 * #css_wrapper_metadata_end */

:host {
  display: block;
  outline: none;
}

#actions-focus-border {
  overflow: hidden;
}

#actions-focus-border:focus-within,
/* Keep outline when chip receives active state after being focused. */
#actions-focus-border:focus-within:has(#action:active),
#actions-focus-border:has(#action.selected),
#actions-focus-border:has(#keyword.selected) {
  outline: 2px solid var(--color-searchbox-results-action-chip-focus-outline);
  border-radius: 10px;
  margin-inline-start: -2px;
}

#actions-focus-border:has(#action:active) {
  outline: none;
}

.container {
  align-items: center;
  cursor: default;
  display: flex;
  overflow: hidden;
  padding-bottom: var(--cr-searchbox-match-padding, 6px);
  padding-inline-end: 16px;
  padding-inline-start: var(--cr-searchbox-match-padding-inline-start, 12px);
  padding-top: var(--cr-searchbox-match-padding, 6px);
  position: relative;
}

.container + .container {
  flex-direction: row;
  margin-inline-start: 40px; /* icon width + text padding */
  padding-top: 0;
  padding-bottom: 12px;
}

.aria-hidden-container {
  /* Ignore container for visual layout. */
  display: contents;
}

:host([has-action]) .container {
  height: 38px;
  padding-top: 3px;
  padding-bottom: 3px;
}

:host([is-top-chrome-searchbox_]:is([has-action],[has-keyword-chip]))
    .container {
  height: 40px;
  padding-top: 0;
  padding-bottom: 0;
}

:host(:not([is-lens-searchbox_])) .container:not(.actions) {
  margin-inline-end: 16px;
  border-start-end-radius: 24px;
  border-end-end-radius: 24px;
}

:host-context([has-secondary-side]):host-context([can-show-secondary-side]) .container:not(.actions) {
  margin-inline-end: 0;
}

.container:not(.actions):hover {
  background-color: var(--color-searchbox-results-background-hovered);
}

:host(:is(:focus-visible, [selected]))  .container:not(.actions) {
  background-color: var(--color-searchbox-results-background-selected,
      var(--color-searchbox-results-background-hovered));
}

@media (forced-colors: active) {
  :host([is-top-chrome-searchbox_]) .container:not(.actions):hover,
  :host([is-top-chrome-searchbox_]:is(:focus-visible, [selected])) .container:not(.actions) {
    background-color: Highlight;
  }
}

:host([enable-csb-motion-tweaks_][is-lens-searchbox_]) .container {
  height: 48px;
  padding-bottom: 0;
  padding-top: 0;
}

.actions.container {
  align-self: center;
  flex-grow: 1;
  flex-shrink: 0;
  padding-bottom: 0;
  padding-inline-end: 0;
  padding-inline-start: 0;
  padding-top: 0;
  display: none;
}

:host-context(.vertical) .actions.container {
  display: flex;
}

:host([has-action]) .actions.container {
  padding-inline-end: 8px;
  padding-inline-start: 8px;
}

#contents,
#description {
  overflow: hidden;
  text-overflow: ellipsis;
}

#ellipsis {
  inset-inline-end: 0;
  position: absolute;
}

:host([show-thumbnail]) #ellipsis {
  position: relative;
}

#focusIndicator {
  --searchbox-match-focus-indicator-width_: 7px;
  background-color: var(--color-searchbox-results-focus-indicator);
  border-radius: 3px;
  display: none;
  height: 100%;
  inset-inline-start: round(up, calc(-1 * var(--searchbox-match-focus-indicator-width_) / 2), 1px);
  position: absolute;
  width: var(--searchbox-match-focus-indicator-width_);
}

/* Focus indicator only shown for vertically rendered matches. */
:host-context(.vertical):host(:is(:focus-visible, [selected]:not(:focus-within))) #focusIndicator:not(.selected-within) {
  display: block;
}

:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])) #icon {
  --color-searchbox-search-icon-background:
      var(--color-searchbox-results-dim-selected);
}

#prefix {
  opacity: 0;
}

#separator {
  white-space: pre;
}

#tail-suggest-prefix {
   position: relative;
}

#text-container {
  align-items: center;
  display: flex;
  flex-grow: 0;
  overflow: hidden;
  padding-inline-end: var(--cr-searchbox-match-text-padding-inline-end, 8px);
  padding-inline-start: var(--cr-searchbox-match-text-padding-inline-start, 8px);
  white-space: nowrap;

  /* Unlike other platforms, the text in the WebUI omnibox popup appears 2px
     lower than the Views omnibox. See http://crbug.com/452732795. */
  transform: translateY(-2px);

}

#suggestion {
  display: inherit;
  overflow: inherit;
  flex-direction: inherit;
  max-width: 100%;
}

:host([is-top-chrome-searchbox_]) #text-container #suggestion {
  display: inline-block;
  min-width: 0;
  flex: 1 1 auto;
  white-space: nowrap;
  text-overflow: ellipsis;
}

:host([is-lens-searchbox_]) #text-container {
  display: -webkit-box;
  line-clamp: 2;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  white-space: normal;
}

:host([is-lens-searchbox_]) #suggestion {
  /* The above "display: -webkit-box" doesn't get inherited properly, so
     force to display inline to ensure the text clamps correctly. */
  display: inline;
}

:host([has-action]) #text-container {
  padding-inline-end: 4px;
}

:host([is-two-row-suggestion]) #text-container {
  align-items: flex-start;
  flex-direction: column;
}

:host([is-two-row-suggestion]) #separator {
  display: none;
}

:host([is-two-row-suggestion]) #contents,
:host([is-two-row-suggestion]) #description {
  width: 100%;
}

/* Deemphasizes description for two-row suggestions. */
:host([is-two-row-suggestion]) #description {
  font-size: .875em;
}

:host([is-contextual-suggestion_]:not([show-contextual-description])) #description,
:host([is-contextual-suggestion_]:not([show-contextual-description])) #separator {
  display: none;
}

:host([is-contextual-suggestion_]) .container:not(.actions):hover #description,
:host([is-contextual-suggestion_]) .container:not(.actions):hover #separator,
:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])):host([is-contextual-suggestion_]) #description,
:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])):host([is-contextual-suggestion_]) #separator {
  display: inline;
}

.match {
  font-weight: var(--cr-searchbox-match-font-weight, 600);
}

/* In the lens side panel searchbox, the typed prefix of the match should
 * have a different color than the rest of the autocomplete suggestion. */
:host(:not([is-top-chrome-searchbox_])) #contents span:not(.match),
#ellipsis {
  color: var(--color-searchbox-results-typed-prefix, --color-searchbox-results-foreground);
}

/* Used to override contents color in zero prefix matches */
:host-context([has-empty-input]) #contents span,
:host-context([has-empty-input]) #ellipsis {
  color: var(--color-searchbox-results-foreground);
}

/* Use dimmed color for descriptions. */
#description,
.dim {
  color: var(--color-searchbox-results-foreground-dimmed);
}

/* Uses a dimmed color for description for entities. */
:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])):host([is-entity-suggestion]) #description,
:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])) .dim {
  color: var(--color-searchbox-results-dim-selected);
}

/* URL ellipsis color to match the URL text color */
#description:has(.url),
.url {
  color: var(--color-searchbox-results-url);
}

:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])) .url {
  color: var(--color-searchbox-results-url-selected);
}

#remove {
  display: none;
  margin-inline-end: 1px;
}

:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])) #remove {
  --cr-icon-button-fill-color: var(--color-searchbox-results-icon-selected);
}


:host-context(cr-searchbox-match:-webkit-any(:focus-within, [selected])) #remove:hover {
  --cr-icon-button-hover-background-color:
      var(--color-searchbox-results-button-selected-hover);
}

:host-context(.vertical) .container:hover #remove:not([hidden]),
:host-context(cr-searchbox-match[selected]):host-context(.vertical) #remove:not([hidden]),
#remove:focus-visible:not([hidden]),
#remove.selected:not([hidden]) {
  display: inline-flex;
}

.selected:not(#action):not(#keyword) {
  box-shadow: inset 0 0 0 2px
      var(--color-searchbox-results-icon-focused-outline);
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image]),
:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image]) .container {
  border-radius: 16px;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    .container {
  box-sizing: border-box;
  flex-direction: column;
  margin-inline-end: 0;
  padding: 6px;
  padding-block-end: 16px;
  width: 102px;
  height: auto;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    .focus-indicator {
  display: none;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #icon {
  --cr-searchbox-icon-border-radius: 12px;
  /* Disable placeholder dominant color as the images are large and the
   * placeholder color looks like a flash of unstyled content. */
  --color-searchbox-results-icon-container-background: transparent;
  height: 90px;
  margin-block-end: 8px;
  width: 90px;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #text-container {
  padding: 0;
  white-space: normal;
  width: 100%;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #contents,
:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #description {
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
  display: -webkit-box;
  font-weight: 400;
  overflow: hidden;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #contents {
  font-size: 13px;
  line-height: 20px;
  margin-block-end: 4px;
}

:host-context(.secondary-side):host-context(.horizontal):host([is-entity-suggestion][has-image])
    #description {
  font-size: 12px;
  line-height: 16px;
}
`])];
}
function getHtml17() {
  return b2`<!--_html_template_start_-->
<div class="container">
  <div class="aria-hidden-container" aria-hidden="true">
    <div id="focusIndicator" class="${this.getFocusIndicatorCssClass_()}"></div>
    <cr-searchbox-icon id="icon" .match="${this.match}"></cr-searchbox-icon>
    <div id="text-container">
      <span id="tail-suggest-prefix" ?hidden="${!this.tailSuggestPrefix_}">
        <span id="prefix">${this.tailSuggestPrefix_}</span>
        <!-- This is equivalent to AutocompleteMatch::kEllipsis which is
            prepended to the match content in other surfaces-->
        <span id="ellipsis">...&nbsp;</span>
      </span>
      <!-- When a thumbnail is in the searchbox all results should have an
          ellipsis prepended to the suggestion. -->
      <span id="ellipsis" ?hidden="${!this.showEllipsis}">...&nbsp;</span>
      <span id="suggestion">
        <span id="contents" .innerHTML="${this.contentsHtml_}"></span>
        <span id="separator" class="dim">${this.separatorText_}</span>
        <span id="description" .innerHTML="${this.descriptionHtml_}"></span>
      </span>
    </div>
  </div>

  ${this.match.keywordModel?.type === KeywordType.kChip ? b2`
    <div id="actions-focus-border">
      <cr-searchbox-action id="keyword"
          class="${this.getKeywordCssClass_()}"
          hint="${this.match.keywordModel.chipHint}"
          icon-path="//resources/images/icon_search.svg"
          aria-label="${this.match.keywordModel.chipA11y}"
          @execute-action="${this.onKeywordExecuteAction_}"
          tabindex="${this.virtualFocusEnabled ? -1 : 1}">
      </cr-searchbox-action>
    </div>
  ` : ""}
  <div id="actions-container" class="actions container">
    ${this.match.actions.map((item, index) => b2`
      <div id="actions-focus-border">
        <cr-searchbox-action id="action"
            class="${this.getActionCssClass_(index)}"
            hint="${item.hint}"
            suggestion-contents="${item.suggestionContents}"
            icon-path="${item.iconPath}"
            aria-label="${item.a11yLabel}"
            action-index="${index}"
            @execute-action="${this.onExecuteAction_}"
            tabindex="${this.virtualFocusEnabled ? -1 : 2}">
        </cr-searchbox-action>
      </div>
    `)}
  </div>
  <cr-icon-button id="remove"
      class="action-icon icon-clear ${this.getRemoveCssClass_()}"
      tabindex="${this.virtualFocusEnabled ? -1 : 3}"
      aria-label="${this.removeButtonAriaLabel_}"
      title="${this.removeButtonTitle_}"
      ?hidden="${!this.match.supportsDeletion}"
      @click="${this.onRemoveButtonClick_}"
      @mousedown="${this.onRemoveButtonMousedown_}">
  </cr-icon-button>
</div>
<!--_html_template_end_-->`;
}
function selectionsEqual(a3, b3) {
  return a3.line === b3.line && a3.state === b3.state && a3.actionIndex === b3.actionIndex;
}
function findSelectionIndex(selections, target) {
  let index = selections.findIndex((s5) => selectionsEqual(target, s5));
  if (index < 0 && target.state === SelectionLineState.kKeywordMode) {
    index = selections.findIndex(
      (s5) => selectionsEqual({ ...target, state: SelectionLineState.kNormal }, s5)
    );
  }
  if (index < 0 && target.state === SelectionLineState.kNormal) {
    index = selections.findIndex(
      (s5) => selectionsEqual(
        { ...target, state: SelectionLineState.kKeywordMode },
        s5
      )
    );
  }
  return index;
}
function getSelectionsForMatch(match, matchIndex) {
  if (match.isHidden && !match.allowedToBeDefaultMatch) {
    return [];
  }
  const selections = [{
    line: matchIndex,
    state: match.keywordModel?.type === KeywordType.kInstant ? SelectionLineState.kKeywordMode : SelectionLineState.kNormal,
    actionIndex: 0
  }];
  if (match.keywordModel?.type === KeywordType.kChip) {
    selections.push({
      line: matchIndex,
      state: SelectionLineState.kKeywordMode,
      actionIndex: 0
    });
  }
  if (match.actions) {
    match.actions.forEach((_2, actionIndex) => {
      selections.push({
        line: matchIndex,
        state: SelectionLineState.kFocusedButtonAction,
        actionIndex
      });
    });
  }
  if (match.supportsDeletion) {
    selections.push({
      line: matchIndex,
      state: SelectionLineState.kFocusedButtonRemoveSuggestion,
      actionIndex: 0
    });
  }
  return selections;
}
function getMatchSelections(result) {
  if (!result) {
    return [];
  }
  return result.matches.flatMap(
    (match, matchIndex) => getSelectionsForMatch(match, matchIndex)
  );
}
var SearchboxSelectionMixin = (superClass) => {
  class SearchboxSelectionMixin2 extends superClass {
    selection_ = kDefaultSelection;
    get isAimButtonVisible() {
      return false;
    }
    get showContextEntrypoint() {
      return false;
    }
    get selection() {
      return this.selection_;
    }
    setSelection(selection) {
      if (selectionsEqual(this.selection_, selection)) {
        return;
      }
      const oldSelection = this.selection_;
      this.selection_ = selection;
      this.requestUpdate("selection", oldSelection);
    }
    onSelectionChanged(e5) {
      this.setSelection(e5.detail.value);
    }
    isAiModeVirtualFocused() {
      return this.selection_.state === SelectionLineState.kFocusedButtonAim;
    }
    isContextEntrypointVirtualFocused() {
      return this.selection_.state === SelectionLineState.kFocusedButtonContextEntrypoint;
    }
    getAvailableSelections(result) {
      if (!result) {
        return [];
      }
      const available = getMatchSelections(result);
      if (this.showContextEntrypoint) {
        available.push({
          line: -1,
          state: SelectionLineState.kFocusedButtonContextEntrypoint,
          actionIndex: 0
        });
      }
      if (this.isAimButtonVisible) {
        const insertionIndex = available.length > 0 && result.matches[0]?.allowedToBeDefaultMatch ? 1 : 0;
        available.splice(insertionIndex, 0, {
          line: result.matches.findIndex(
            (m3) => m3.allowedToBeDefaultMatch
          ),
          state: SelectionLineState.kFocusedButtonAim,
          actionIndex: 0
        });
        if (!result.matches[0]?.allowedToBeDefaultMatch) {
          available.splice(0, 0, {
            line: -1,
            state: SelectionLineState.kNormal,
            actionIndex: 0
          });
        }
      }
      return available;
    }
    /**
     * Determines whether stepping from `from` in `direction` with `step`
     * granularity wraps around (cycles) the available selections.
     */
    stepCyclesSelection(result, from, direction, step) {
      const available = this.getAvailableSelections(result);
      if (available.length === 0) {
        return true;
      }
      const fromIndex = findSelectionIndex(available, from);
      if (fromIndex < 0) {
        return direction === SelectionDirection.kBackward;
      }
      const next = this.getNextSelection(result, from, direction, step);
      const nextIndex = findSelectionIndex(available, next);
      if (nextIndex < 0) {
        return true;
      }
      return direction === SelectionDirection.kForward ? nextIndex <= fromIndex : nextIndex >= fromIndex;
    }
    getNextSelection(result, from, direction, step) {
      const available = this.getAvailableSelections(result);
      if (available.length === 0) {
        return from;
      }
      const isNormal = (selection) => (selection.state === SelectionLineState.kNormal || selection.state === SelectionLineState.kKeywordMode && result?.matches[selection.line]?.keywordModel?.type === KeywordType.kInstant) && selection.line !== -1;
      let fromIndex = findSelectionIndex(available, from);
      const selectionsList = [...available];
      if (fromIndex < 0) {
        selectionsList.splice(0, 0, from);
        fromIndex = 0;
      }
      if (step === SelectionStep.kAllLines) {
        if (direction === SelectionDirection.kBackward && from.line === -1) {
          return from;
        }
        const normalIndex = direction === SelectionDirection.kBackward ? selectionsList.findIndex(isNormal) : selectionsList.findLastIndex(isNormal);
        return normalIndex < 0 ? from : selectionsList[normalIndex];
      }
      for (let offset = 1; offset < selectionsList.length; offset++) {
        const offsetDirection = direction === SelectionDirection.kForward ? offset : -offset;
        const newIndex = fromIndex + offsetDirection;
        const remainder2 = (lhs, rhs) => (lhs % rhs + rhs) % rhs;
        const index = remainder2(newIndex, selectionsList.length);
        const selection = selectionsList[index];
        if (step === SelectionStep.kStateOrLine || isNormal(selection)) {
          return selection;
        }
      }
      return from;
    }
  }
  return SearchboxSelectionMixin2;
};
var ENTITY_MATCH_TYPE = "search-suggest-entity";
var kDefaultSelection = {
  line: -1,
  state: SelectionLineState.kNormal,
  actionIndex: 0
};
var SearchboxMatchElement = class extends CrLitElement {
  static get is() {
    return "cr-searchbox-match";
  }
  static get styles() {
    return getCss25();
  }
  render() {
    return getHtml17.bind(this)();
  }
  static get properties() {
    return {
      //========================================================================
      // Public properties
      //========================================================================
      /** Element's 'aria-label' attribute. */
      ariaLabel: {
        type: String,
        reflect: true
      },
      hasAction: {
        type: Boolean,
        reflect: true
      },
      showContextualDescription: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether the match features an image (as opposed to an icon or favicon).
       */
      hasImage: {
        type: Boolean,
        reflect: true
      },
      hasKeywordChip: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether the match is an entity suggestion (with or without an image).
       */
      isEntitySuggestion: {
        type: Boolean,
        reflect: true
      },
      /**
       * Whether the match should be rendered in a two-row layout. Currently
       * limited to matches that feature an image, calculator, and answers.
       */
      isTwoRowSuggestion: {
        type: Boolean,
        reflect: true
      },
      match: { type: Object },
      selection: { type: Object },
      /**
       * Index of the match in the autocomplete result. Used to inform embedder
       * of events such as deletion, click, etc.
       */
      matchIndex: { type: Number },
      showThumbnail: {
        type: Boolean,
        reflect: true
      },
      showEllipsis: { type: Boolean },
      sideType: { type: Number },
      virtualFocusEnabled: { type: Boolean },
      //========================================================================
      // Private properties
      //========================================================================
      isContextualSuggestion_: {
        type: Boolean,
        reflect: true
      },
      isTopChromeSearchbox_: {
        type: Boolean,
        reflect: true
      },
      isLensSearchbox_: {
        type: Boolean,
        reflect: true
      },
      forceHideEllipsis_: { type: Boolean },
      /** Rendered match contents based on autocomplete provided styling. */
      contentsHtml_: { type: String },
      /** Rendered match description based on autocomplete provided styling. */
      descriptionHtml_: { type: String },
      enableCsbMotionTweaks_: {
        type: Boolean,
        reflect: true
      },
      /** Remove button's 'aria-label' attribute. */
      removeButtonAriaLabel_: { type: String },
      removeButtonTitle_: { type: String },
      /** Used to separate the contents from the description. */
      separatorText_: { type: String },
      /** Rendered tail suggest common prefix. */
      tailSuggestPrefix_: { type: String }
    };
  }
  #ariaLabel = "";
  get ariaLabel() {
    return this.#ariaLabel;
  }
  set ariaLabel(_2) {
    this.#ariaLabel = _2;
  }
  #hasAction = false;
  get hasAction() {
    return this.#hasAction;
  }
  set hasAction(_2) {
    this.#hasAction = _2;
  }
  #showContextualDescription = false;
  get showContextualDescription() {
    return this.#showContextualDescription;
  }
  set showContextualDescription(_2) {
    this.#showContextualDescription = _2;
  }
  #hasImage = false;
  get hasImage() {
    return this.#hasImage;
  }
  set hasImage(_2) {
    this.#hasImage = _2;
  }
  #hasKeywordChip = false;
  get hasKeywordChip() {
    return this.#hasKeywordChip;
  }
  set hasKeywordChip(_2) {
    this.#hasKeywordChip = _2;
  }
  #isEntitySuggestion = false;
  get isEntitySuggestion() {
    return this.#isEntitySuggestion;
  }
  set isEntitySuggestion(_2) {
    this.#isEntitySuggestion = _2;
  }
  #isTwoRowSuggestion = false;
  get isTwoRowSuggestion() {
    return this.#isTwoRowSuggestion;
  }
  set isTwoRowSuggestion(_2) {
    this.#isTwoRowSuggestion = _2;
  }
  #match = createAutocompleteMatch();
  get match() {
    return this.#match;
  }
  set match(_2) {
    this.#match = _2;
  }
  #selection = kDefaultSelection;
  get selection() {
    return this.#selection;
  }
  set selection(_2) {
    this.#selection = _2;
  }
  #matchIndex = -1;
  get matchIndex() {
    return this.#matchIndex;
  }
  set matchIndex(_2) {
    this.#matchIndex = _2;
  }
  #sideType = SideType.kDefaultPrimary;
  get sideType() {
    return this.#sideType;
  }
  set sideType(_2) {
    this.#sideType = _2;
  }
  #showThumbnail = false;
  get showThumbnail() {
    return this.#showThumbnail;
  }
  set showThumbnail(_2) {
    this.#showThumbnail = _2;
  }
  #showEllipsis = false;
  get showEllipsis() {
    return this.#showEllipsis;
  }
  set showEllipsis(_2) {
    this.#showEllipsis = _2;
  }
  #virtualFocusEnabled = false;
  get virtualFocusEnabled() {
    return this.#virtualFocusEnabled;
  }
  set virtualFocusEnabled(_2) {
    this.#virtualFocusEnabled = _2;
  }
  #isContextualSuggestion_ = false;
  get isContextualSuggestion_() {
    return this.#isContextualSuggestion_;
  }
  set isContextualSuggestion_(_2) {
    this.#isContextualSuggestion_ = _2;
  }
  #isTopChromeSearchbox_ = loadTimeData.getBoolean("isTopChromeSearchbox");
  get isTopChromeSearchbox_() {
    return this.#isTopChromeSearchbox_;
  }
  set isTopChromeSearchbox_(_2) {
    this.#isTopChromeSearchbox_ = _2;
  }
  #isLensSearchbox_ = loadTimeData.getBoolean("isLensSearchbox");
  get isLensSearchbox_() {
    return this.#isLensSearchbox_;
  }
  set isLensSearchbox_(_2) {
    this.#isLensSearchbox_ = _2;
  }
  #forceHideEllipsis_ = loadTimeData.getBoolean("forceHideEllipsis");
  get forceHideEllipsis_() {
    return this.#forceHideEllipsis_;
  }
  set forceHideEllipsis_(_2) {
    this.#forceHideEllipsis_ = _2;
  }
  #contentsHtml_ = window.trustedTypes.emptyHTML;
  get contentsHtml_() {
    return this.#contentsHtml_;
  }
  set contentsHtml_(_2) {
    this.#contentsHtml_ = _2;
  }
  #descriptionHtml_ = window.trustedTypes.emptyHTML;
  get descriptionHtml_() {
    return this.#descriptionHtml_;
  }
  set descriptionHtml_(_2) {
    this.#descriptionHtml_ = _2;
  }
  #enableCsbMotionTweaks_ = loadTimeData.getBoolean("enableCsbMotionTweaks");
  get enableCsbMotionTweaks_() {
    return this.#enableCsbMotionTweaks_;
  }
  set enableCsbMotionTweaks_(_2) {
    this.#enableCsbMotionTweaks_ = _2;
  }
  #removeButtonAriaLabel_ = "";
  get removeButtonAriaLabel_() {
    return this.#removeButtonAriaLabel_;
  }
  set removeButtonAriaLabel_(_2) {
    this.#removeButtonAriaLabel_ = _2;
  }
  #removeButtonTitle_ = loadTimeData.getString("removeSuggestion");
  get removeButtonTitle_() {
    return this.#removeButtonTitle_;
  }
  set removeButtonTitle_(_2) {
    this.#removeButtonTitle_ = _2;
  }
  #separatorText_ = "";
  get separatorText_() {
    return this.#separatorText_;
  }
  set separatorText_(_2) {
    this.#separatorText_ = _2;
  }
  #tailSuggestPrefix_ = "";
  get tailSuggestPrefix_() {
    return this.#tailSuggestPrefix_;
  }
  set tailSuggestPrefix_(_2) {
    this.#tailSuggestPrefix_ = _2;
  }
  pageHandler_;
  constructor() {
    super();
    this.pageHandler_ = SearchboxBrowserProxy.getInstance().handler;
  }
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("match")) {
      this.ariaLabel = this.computeAriaLabel_();
      this.contentsHtml_ = this.computeContentsHtml_();
      this.descriptionHtml_ = this.computeDescriptionHtml_();
      this.hasAction = this.computeHasAction_();
      this.hasKeywordChip = this.computeHasKeywordChip_();
      this.hasImage = this.computeHasImage_();
      this.isContextualSuggestion_ = this.computeIsContextualSuggestion_();
      this.isEntitySuggestion = this.computeIsEntitySuggestion_();
      this.isTwoRowSuggestion = this.computeIsTwoRowSuggestion_();
      this.removeButtonAriaLabel_ = this.computeRemoveButtonAriaLabel_();
      this.separatorText_ = this.computeSeparatorText_();
      this.tailSuggestPrefix_ = this.computeTailSuggestPrefix_();
      this.selection = kDefaultSelection;
    }
    const changedPrivateProperties = changedProperties;
    if (changedProperties.has("showThumbnail") || changedPrivateProperties.has("isLensSearchbox_") || changedPrivateProperties.has("forceHideEllipsis_")) {
      this.showEllipsis = this.computeShowEllipsis_();
    }
  }
  firstUpdated() {
    this.addEventListener("click", (event) => this.onMatchClick_(event));
    this.addEventListener("auxclick", (event) => this.onMatchClick_(event));
    this.addEventListener("focusin", () => this.onMatchFocusin_());
    this.addEventListener(
      "mousedown",
      (event) => this.onMatchMouseDown_(event)
    );
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    if (changedProperties.has("selection") || changedProperties.has("match")) {
      this.updateAriaLabel_();
    }
    if (this.virtualFocusEnabled && this.selection.line === this.matchIndex && this.selection.state !== SelectionLineState.kFocusedButtonAim) {
      const oldSelection = changedProperties.get("selection");
      if (changedProperties.has("selection") && (!oldSelection || !selectionsEqual(oldSelection, this.selection))) {
        announce(this, this.ariaLabel);
      }
    }
  }
  updateAriaLabel_() {
    if (!this.virtualFocusEnabled) {
      return;
    }
    const state = this.selection.state;
    if (this.selection.line === this.matchIndex) {
      if (state === SelectionLineState.kNormal) {
        this.ariaLabel = this.computeAriaLabel_();
      } else if (state === SelectionLineState.kKeywordMode) {
        this.ariaLabel = this.match.keywordModel?.chipA11y || "";
      } else if (state === SelectionLineState.kFocusedButtonAction) {
        const action = this.match.actions[this.selection.actionIndex];
        this.ariaLabel = action ? action.a11yLabel : "";
      } else if (state === SelectionLineState.kFocusedButtonRemoveSuggestion) {
        this.ariaLabel = this.removeButtonAriaLabel_ || "";
      }
    } else {
      this.ariaLabel = this.computeAriaLabel_();
    }
  }
  //============================================================================
  // Event handlers
  //============================================================================
  onKeywordExecuteAction_(e5) {
    const event = e5.detail.event;
    this.fire(
      "keyword-click",
      { match: this.match, matchIndex: this.matchIndex }
    );
    this.pageHandler_.activateKeyword(
      this.matchIndex,
      this.match.destinationUrl,
      mojoTimeTicks(Date.now()),
      // Distinguish mouse and touch or pen events for logging purposes.
      event.pointerType === "mouse"
    );
  }
  /**
   * containing index of the action that was removed as well as modifier key
   * presses.
   */
  onExecuteAction_(e5) {
    const event = e5.detail.event;
    this.pageHandler_.executeAction(
      this.matchIndex,
      e5.detail.actionIndex,
      this.match.destinationUrl,
      mojoTimeTicks(Date.now()),
      event.button || 0,
      event.altKey,
      event.ctrlKey,
      event.metaKey,
      event.shiftKey
    );
  }
  onMatchClick_(e5) {
    if (e5.button > 1) {
      return;
    }
    e5.preventDefault();
    e5.stopPropagation();
    if (this.match.keywordModel?.type === KeywordType.kInstant) {
      this.fire(
        "keyword-click",
        { match: this.match, matchIndex: this.matchIndex }
      );
      this.pageHandler_.activateKeyword(
        this.matchIndex,
        this.match.destinationUrl,
        mojoTimeTicks(Date.now()),
        e5.button === 0
      );
      return;
    }
    this.pageHandler_.openAutocompleteMatch(
      this.matchIndex,
      this.match.destinationUrl,
      /*areMatchesShowing=*/
      true,
      /*mouseButton=*/
      e5.button || 0,
      {
        altKey: e5.altKey,
        ctrlKey: e5.ctrlKey,
        metaKey: e5.metaKey,
        shiftKey: e5.shiftKey
      },
      /*viaKeyboard=*/
      false
    );
    const backgroundTab = (e5.metaKey || e5.ctrlKey) && e5.shiftKey;
    if (!backgroundTab) {
      this.fire("match-click");
    }
  }
  onMatchFocusin_() {
    this.fire("match-focusin", this.matchIndex);
  }
  onMatchMouseDown_(e5) {
    if (this.match.keywordModel?.type === KeywordType.kInstant) {
      e5.preventDefault();
    }
    this.pageHandler_.onNavigationLikely(
      this.matchIndex,
      this.match.destinationUrl,
      NavigationPredictor.kMouseDown
    );
  }
  onRemoveButtonClick_(e5) {
    if (e5.button !== 0) {
      return;
    }
    e5.preventDefault();
    e5.stopPropagation();
    this.fire("match-remove");
    this.pageHandler_.deleteAutocompleteMatch(
      this.matchIndex,
      this.match.destinationUrl
    );
  }
  onRemoveButtonMousedown_(e5) {
    e5.preventDefault();
  }
  //============================================================================
  // Helpers
  //============================================================================
  computeAriaLabel_() {
    if (!this.match) {
      return "";
    }
    let label = this.match.a11yLabel;
    const description = this.getMatchDescription_();
    if (description) {
      label = `${label}, ${description}`;
    }
    return label;
  }
  /**
   * Sanitizes .innerHTML from `renderTextWithClassifications_()` through
   * `sanitizeInnerHtml` to ensure it only contains allowed tags.
   * @param innerHtml The .innerHTML from `renderTextWithClassifications_()`
   * @return Sanitized TrustedHTML safe for rendering
   */
  sanitizeInnerHtml_(innerHtml) {
    try {
      return sanitizeInnerHtml(innerHtml, { attrs: ["class"] });
    } catch (e5) {
      return window.trustedTypes.emptyHTML;
    }
  }
  computeContentsHtml_() {
    if (!this.match) {
      return window.trustedTypes.emptyHTML;
    }
    return this.sanitizeInnerHtml_(
      this.renderTextWithClassifications_(
        this.getMatchContents_(),
        this.getMatchContentsClassifications_()
      ).innerHTML
    );
  }
  computeDescriptionHtml_() {
    if (!this.match) {
      return window.trustedTypes.emptyHTML;
    }
    return this.sanitizeInnerHtml_(
      this.renderTextWithClassifications_(
        this.getMatchDescription_(),
        this.getMatchDescriptionClassifications_()
      ).innerHTML
    );
  }
  computeHasAction_() {
    return this.match?.actions?.length > 0;
  }
  computeHasKeywordChip_() {
    return this.match?.keywordModel?.type === KeywordType.kChip;
  }
  computeHasImage_() {
    return this.match && !!this.match.imageUrl;
  }
  computeIsContextualSuggestion_() {
    return this.match.isContextualSuggestion;
  }
  computeIsEntitySuggestion_() {
    return this.match && this.match.type === ENTITY_MATCH_TYPE;
  }
  computeIsTwoRowSuggestion_() {
    return !this.isTopChromeSearchbox_ && this.match && this.match.isTwoRowSuggestion;
  }
  computeRemoveButtonAriaLabel_() {
    if (!this.match) {
      return "";
    }
    return this.match.removeButtonA11yLabel;
  }
  computeSeparatorText_() {
    return this.getMatchDescription_() ? loadTimeData.getString("searchboxSeparator") : "";
  }
  computeTailSuggestPrefix_() {
    if (!this.match || !this.match.tailSuggestCommonPrefix) {
      return "";
    }
    const prefix = this.match.tailSuggestCommonPrefix;
    if (prefix.slice(-1) === " ") {
      return prefix.slice(0, -1) + "\xA0";
    }
    return prefix;
  }
  computeShowEllipsis_() {
    if (this.isLensSearchbox_ && this.forceHideEllipsis_) {
      return false;
    }
    return this.showThumbnail;
  }
  /**
   * Decodes the AcMatchClassificationStyle entries encoded in the given
   * ACMatchClassification style field, maps each entry to a CSS
   * class and returns them.
   */
  convertClassificationStyleToCssClasses_(style2) {
    const classes = [];
    if (style2 & 4) {
      classes.push("dim");
    }
    if (style2 & 2) {
      classes.push("match");
    }
    if (style2 & 1) {
      classes.push("url");
    }
    return classes;
  }
  createSpanWithClasses_(text, classes) {
    const span = document.createElement("span");
    if (classes.length) {
      span.classList.add(...classes);
    }
    span.textContent = text;
    return span;
  }
  /**
   * Renders |text| based on the given ACMatchClassification(s)
   * Each classification contains an 'offset' and an encoded list of styles for
   * styling a substring starting with the 'offset' and ending with the next.
   * @return A <span> with <span> children for each styled substring.
   */
  renderTextWithClassifications_(text, classifications) {
    const container = document.createElement("span");
    if (classifications.length === 0) {
      container.appendChild(this.createSpanWithClasses_(text, []));
      return container;
    }
    const firstClassification = classifications[0];
    if (firstClassification.offset > 0) {
      const prefix = text.substring(0, firstClassification.offset);
      container.appendChild(this.createSpanWithClasses_(prefix, []));
    }
    classifications.map(({ offset, style: style2 }, index) => {
      const nextOffset = index + 1 < classifications.length ? classifications[index + 1].offset : text.length;
      const subString = text.substring(offset, nextOffset);
      const classes = this.convertClassificationStyleToCssClasses_(style2);
      container.appendChild(this.createSpanWithClasses_(subString, classes));
    });
    return container;
  }
  getMatchContents_() {
    if (!this.match) {
      return "";
    }
    const match = this.match;
    const matchContents = match.contents;
    const matchDescription = match.description;
    return match.swapContentsAndDescription ? matchDescription : matchContents;
  }
  getMatchDescription_() {
    if (!this.match) {
      return "";
    }
    const match = this.match;
    const matchContents = match.contents;
    const matchDescription = match.description;
    return match.swapContentsAndDescription ? matchContents : matchDescription;
  }
  getMatchContentsClassifications_() {
    if (!this.match) {
      return [];
    }
    const match = this.match;
    return match.swapContentsAndDescription ? match.descriptionClass : match.contentsClass;
  }
  getMatchDescriptionClassifications_() {
    if (!this.match) {
      return [];
    }
    const match = this.match;
    return match.swapContentsAndDescription ? match.contentsClass : match.descriptionClass;
  }
  getFocusIndicatorCssClass_() {
    return this.selection.line === this.matchIndex && this.selection.state !== SelectionLineState.kNormal && this.match.keywordModel?.type !== KeywordType.kInstant ? "selected-within" : "";
  }
  getKeywordCssClass_() {
    return this.selection.line === this.matchIndex && this.selection.state === SelectionLineState.kKeywordMode ? "selected" : "";
  }
  getActionCssClass_(actionIndex) {
    return this.selection.line === this.matchIndex && this.selection.state === SelectionLineState.kFocusedButtonAction && this.selection.actionIndex === actionIndex ? "selected" : "";
  }
  getRemoveCssClass_() {
    return this.selection.line === this.matchIndex && this.selection.state === SelectionLineState.kFocusedButtonRemoveSuggestion ? "selected" : "";
  }
};
customElements.define(SearchboxMatchElement.is, SearchboxMatchElement);
function getCss26() {
  return [getCss7(), getCss24(), i(["/* Copyright 2025 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #import=//resources/cr_elements/cr_icons_lit.css.js\n * #import=./searchbox_dropdown_shared_style.css.js\n * #scheme=relative\n * #include=cr-icons-lit searchbox-dropdown-shared-style\n * #css_wrapper_metadata_end */\n\n:host {\n  user-select: none;\n}\n\n#content {\n  background-color: var(--color-searchbox-results-background);\n  border-radius: calc(0.5 * var(--cr-searchbox-height));\n  box-shadow: var(--cr-searchbox-shadow);\n  display: flex;\n  gap: 16px;\n  margin-bottom: var(--cr-searchbox-results-margin-bottom, 8px);\n  overflow: hidden;\n  padding-bottom: var(--cr-searchbox-results-padding-bottom, 8px);\n  padding-top: var(--cr-searchbox-height);\n}\n\n@media (forced-colors: active) {\n  #content {\n    border: 1px solid ActiveBorder;\n  }\n}\n\n.matches {\n  display: contents;\n}\n\ncr-searchbox-match {\n  color: var(--color-searchbox-results-foreground);\n}\n\ncr-searchbox-match:-webkit-any(:focus-within, [selected]) {\n  color: var(--color-searchbox-results-foreground-selected,\n      var(--color-searchbox-results-foreground));\n}\n\n.header {\n  align-items: center;\n  box-sizing: border-box;\n  display: flex;\n  font-size: inherit;\n  font-weight: inherit;\n  /* To mirror match typical row height of items in vertical list. */\n  height: 44px;\n  margin-block-end: 0;\n  margin-block-start: 0;\n  outline: none;\n  padding-bottom: 6px;\n  padding-inline-end: 16px;\n  padding-inline-start: var(--cr-searchbox-dropdown-header-padding-inline-start, 12px);\n  padding-top: 6px;\n}\n\n.header .text {\n  color: var(--color-searchbox-results-foreground-dimmed);\n  font-size: .875em;\n  font-weight: 500;\n  overflow: hidden;\n  padding-inline-end: 1px;\n  text-overflow: ellipsis;\n  white-space: nowrap;\n  z-index: 1;\n}\n\n@media (forced-colors: active) {\n  cr-searchbox-match:-webkit-any(:hover, :focus-within, [selected]) {\n    background-color: Highlight;\n  }\n}\n\n.primary-side {\n  flex: 1;\n  min-width: 0;\n}\n\n:host-context([is-lens-searchbox_]) .primary-side::before {\n  content: '';\n  position: relative;\n  height: 1px;\n  background-color: var(--color-searchbox-dropdown-divider);\n  top: 0;\n  width: calc(var(--cr-searchbox-width) - 24px);\n  display: block;\n  inset-inline-start: 12px;\n  margin-block-end: 4px;\n}\n\n.secondary-side {\n  display: var(--cr-searchbox-secondary-side-display, none);\n  min-width: 0;\n  padding-block-end: 8px;\n  padding-inline-end: 16px;\n  width: 314px;\n}\n\n.secondary-side .header {\n  padding-inline-end: 0;\n  padding-inline-start: 0;\n}\n\n.secondary-side .matches {\n  display: block;\n}\n\n.secondary-side .matches.horizontal {\n  display: flex;\n  gap: 4px;\n}\n"])];
}
function getHtml18() {
  return b2`<!--_html_template_start_-->
<div id="content" part="dropdown-content">
${this.sideTypes_().map((sideType) => b2`
  <div class="${this.sideTypeClass_(sideType)}">
    ${this.groupIdsForSideType_(sideType).map((groupId) => b2`
      ${this.hasHeaderForGroup_(groupId) ? b2`
        <!-- Header cannot be tabbed into but gets focus when clicked. This
            stops the dropdown from losing focus and closing as a result. -->
        <h3 class="header" data-id="${groupId}" tabindex="-1"
            id="hg_${groupId}"
            @mousedown="${this.onHeaderMousedown_}">
            <span class="text">${this.headerForGroup_(groupId)}</span>
        </h3>
      ` : ""}
      <div class="matches ${this.renderTypeClassForGroup_(groupId)}">
      ${this.matchesForGroup_(groupId).map((match) => b2`
        <cr-searchbox-match
            id="match-${this.matchIndex_(match)}"
            .selection="${this.selection}"
            .virtualFocusEnabled="${this.virtualFocusEnabled}"
            tabindex="${this.virtualFocusEnabled ? -1 : 0}"
            role="option"
            aria-describedby="${this.getAriaDescribedByForGroup_(groupId)}"
            .match="${match}" match-index="${this.matchIndex_(match)}"
            side-type="${sideType}"
            ?show-contextual-description="${match.showContextualDescription}"
            ?selected="${this.isSelected_(match)}"
            ?show-thumbnail="${this.showThumbnail}">
        </cr-searchbox-match>
      `)}
      </div>
    `)}
  </div>
`)}
</div>
<!--_html_template_end_-->`;
}
var remainder = (lhs, rhs) => (lhs % rhs + rhs) % rhs;
var SearchboxDropdownElement = class extends CrLitElement {
  static get is() {
    return "cr-searchbox-dropdown";
  }
  static get styles() {
    return getCss26();
  }
  render() {
    return getHtml18.bind(this)();
  }
  static get properties() {
    return {
      //========================================================================
      // Public properties
      //========================================================================
      /**
       * Whether the secondary side can be shown based on the feature state and
       * the width available to the dropdown.
       */
      canShowSecondarySide: { type: Boolean },
      /**
       * Whether the secondary side was at any point available to be shown.
       */
      hadSecondarySide: {
        type: Boolean,
        notify: true
      },
      /*
       * Whether the secondary side is currently available to be shown.
       */
      hasSecondarySide: {
        type: Boolean,
        notify: true,
        reflect: true
      },
      hasEmptyInput: {
        type: Boolean,
        reflect: true
      },
      result: { type: Object },
      // TODO(crbug.com/519713849): Remove selectedMatchIndex once
      // kRealboxVirtualFocusNavigation is launched.
      selectedMatchIndex: {
        type: Number,
        notify: true
      },
      /** Omnibox focused selection state. */
      selection: {
        type: Object,
        notify: true
      },
      showThumbnail: { type: Boolean },
      virtualFocusEnabled: { type: Boolean },
      //========================================================================
      // Private properties
      //========================================================================
      /**
       * Computed value for whether or not the dropdown should show the
       * secondary side. This depends on whether the parent has set
       * `canShowSecondarySide` to true and whether there are visible primary
       * matches.
       */
      showSecondarySide_: { type: Boolean }
    };
  }
  #canShowSecondarySide = false;
  get canShowSecondarySide() {
    return this.#canShowSecondarySide;
  }
  set canShowSecondarySide(_2) {
    this.#canShowSecondarySide = _2;
  }
  #hadSecondarySide = false;
  get hadSecondarySide() {
    return this.#hadSecondarySide;
  }
  set hadSecondarySide(_2) {
    this.#hadSecondarySide = _2;
  }
  #hasSecondarySide = false;
  get hasSecondarySide() {
    return this.#hasSecondarySide;
  }
  set hasSecondarySide(_2) {
    this.#hasSecondarySide = _2;
  }
  #hasEmptyInput = false;
  get hasEmptyInput() {
    return this.#hasEmptyInput;
  }
  set hasEmptyInput(_2) {
    this.#hasEmptyInput = _2;
  }
  #result = null;
  get result() {
    return this.#result;
  }
  set result(_2) {
    this.#result = _2;
  }
  // TODO(crbug.com/519713849): Remove selectedMatchIndex once
  // kRealboxVirtualFocusNavigation is launched.
  #selectedMatchIndex = -1;
  get selectedMatchIndex() {
    return this.#selectedMatchIndex;
  }
  set selectedMatchIndex(_2) {
    this.#selectedMatchIndex = _2;
  }
  #selection = kDefaultSelection;
  get selection() {
    return this.#selection;
  }
  set selection(_2) {
    this.#selection = _2;
  }
  #showThumbnail = false;
  get showThumbnail() {
    return this.#showThumbnail;
  }
  set showThumbnail(_2) {
    this.#showThumbnail = _2;
  }
  #virtualFocusEnabled = loadTimeData.valueExists("realboxVirtualFocusNavigation") && loadTimeData.getBoolean("realboxVirtualFocusNavigation");
  get virtualFocusEnabled() {
    return this.#virtualFocusEnabled;
  }
  set virtualFocusEnabled(_2) {
    this.#virtualFocusEnabled = _2;
  }
  #showSecondarySide_ = false;
  get showSecondarySide_() {
    return this.#showSecondarySide_;
  }
  set showSecondarySide_(_2) {
    this.#showSecondarySide_ = _2;
  }
  /** The list of selectable match elements. */
  selectableMatchElements_ = [];
  willUpdate(changedProperties) {
    super.willUpdate(changedProperties);
    if (changedProperties.has("result")) {
      this.hasSecondarySide = this.computeHasSecondarySide_();
      this.hasEmptyInput = this.computeHasEmptyInput_();
    }
    if (changedProperties.has("result") || changedProperties.has("canShowSecondarySide")) {
      this.showSecondarySide_ = this.computeShowSecondarySide_();
    }
    if (changedProperties.has("result") && this.virtualFocusEnabled) {
      if (this.selection.state !== SelectionLineState.kNormal && this.selection.state !== SelectionLineState.kFocusedButtonAim) {
        this.selection = {
          line: this.selection.line,
          state: SelectionLineState.kNormal,
          actionIndex: 0
        };
        this.fire("selection-changed", { value: this.selection });
      }
    }
  }
  updated(changedProperties) {
    super.updated(changedProperties);
    this.onResultRepaint_();
    this.selectableMatchElements_ = [...this.shadowRoot.querySelectorAll("cr-searchbox-match")];
  }
  //============================================================================
  // Public methods
  //============================================================================
  /** Filters out secondary matches, if any, unless they can be shown. */
  get selectableMatchElements() {
    return this.selectableMatchElements_.filter(
      (matchEl) => matchEl.sideType === SideType.kDefaultPrimary || this.showSecondarySide_
    );
  }
  /** Unselects the currently selected match, if any. */
  unselect() {
    this.selectedMatchIndex = -1;
    this.selection = kDefaultSelection;
    if (this.virtualFocusEnabled) {
      this.fire("selection-changed", { value: this.selection });
    }
  }
  /** Focuses the selected match, if any. */
  focusSelected() {
    this.selectableMatchElements[this.selectedMatchIndex]?.focus();
  }
  /** Selects the first match. */
  selectFirst() {
    return this.selectIndex(0);
  }
  /** Selects the match at the given index. */
  selectIndex(index) {
    this.selectedMatchIndex = index;
    if (this.virtualFocusEnabled) {
      const match = this.result?.matches[index];
      const newSelection = match && (!match.isHidden || match.allowedToBeDefaultMatch) ? {
        line: index,
        state: SelectionLineState.kNormal,
        actionIndex: 0
      } : kDefaultSelection;
      if (!selectionsEqual(this.selection, newSelection)) {
        this.selection = newSelection;
        this.fire("selection-changed", { value: this.selection });
      }
    }
    return this.updateComplete;
  }
  updateSelection(_oldSelection, selection) {
    this.selectIndex(selection.line);
    this.selection = selection;
  }
  /**
   * Selects the previous match with respect to the currently selected one.
   * Selects the last match if the first one or no match is currently selected.
   */
  selectPrevious() {
    const previous = Math.max(this.selectedMatchIndex, 0) - 1;
    this.selectedMatchIndex = remainder(previous, this.selectableMatchElements.length);
    return this.updateComplete;
  }
  /** Selects the last match. */
  selectLast() {
    this.selectedMatchIndex = this.selectableMatchElements.length - 1;
    return this.updateComplete;
  }
  /**
   * Selects the next match with respect to the currently selected one.
   * Selects the first match if the last one or no match is currently selected.
   */
  selectNext() {
    const next = this.selectedMatchIndex + 1;
    this.selectedMatchIndex = remainder(next, this.selectableMatchElements.length);
    return this.updateComplete;
  }
  //============================================================================
  // Event handlers
  //============================================================================
  onHeaderMousedown_(e5) {
    e5.preventDefault();
  }
  onResultRepaint_() {
    if (!loadTimeData.getBoolean("reportMetrics")) {
      return;
    }
    const metricsReporter = MetricsReporterImpl.getInstance();
    metricsReporter.measure("CharTyped").then((duration) => {
      metricsReporter.umaReportTime(
        loadTimeData.getString("charTypedToPaintMetricName"),
        duration
      );
    }).then(() => {
      metricsReporter.clearMark("CharTyped");
    }).catch(() => {
    });
    metricsReporter.measure("ResultChanged").then((duration) => {
      metricsReporter.umaReportTime(
        loadTimeData.getString("resultChangedToPaintMetricName"),
        duration
      );
    }).then(() => {
      metricsReporter.clearMark("ResultChanged");
    }).catch(() => {
    });
  }
  //============================================================================
  // Helpers
  //============================================================================
  sideTypeClass_(side) {
    return sideTypeToClass(side);
  }
  renderTypeClassForGroup_(groupId) {
    return renderTypeToClass(
      this.result?.suggestionGroupsMap[groupId]?.renderType ?? RenderType.kDefaultVertical
    );
  }
  computeHasSecondarySide_() {
    const hasSecondarySide = !!this.groupIdsForSideType_(SideType.kSecondary).length;
    if (!this.hadSecondarySide) {
      this.hadSecondarySide = hasSecondarySide;
    }
    return hasSecondarySide;
  }
  computeHasEmptyInput_() {
    return !!this.result && this.result.input === "";
  }
  isSelected_(match) {
    const selectedIndex = this.virtualFocusEnabled ? this.selection.line : this.selectedMatchIndex;
    return this.matchIndex_(match) === selectedIndex;
  }
  /**
   * @returns The unique suggestion group IDs that belong to the given side type
   *     while preserving the order in which they appear in the list of matches.
   */
  groupIdsForSideType_(side) {
    return [...new Set(
      this.result?.matches.map((match) => match.suggestionGroupId).filter((groupId) => this.sideTypeForGroup_(groupId) === side)
    )];
  }
  /**
   * @returns Whether the given suggestion group ID has a header.
   */
  hasHeaderForGroup_(groupId) {
    return !!this.headerForGroup_(groupId);
  }
  getAriaDescribedByForGroup_(groupId) {
    return this.hasHeaderForGroup_(groupId) ? `hg_${groupId}` : "";
  }
  /**
   * @returns The header for the given suggestion group ID, if any.
   */
  headerForGroup_(groupId) {
    return this.result?.suggestionGroupsMap[groupId] ? this.result.suggestionGroupsMap[groupId].header : "";
  }
  /**
   * @returns Index of the match in the autocomplete result. Passed to the match
   *     so it knows its position in the list of matches.
   */
  matchIndex_(match) {
    return this.result?.matches.indexOf(match) ?? -1;
  }
  /**
   * @returns The list of visible matches that belong to the given suggestion
   *     group ID.
   */
  matchesForGroup_(groupId) {
    return (this.result?.matches ?? []).filter(
      (match) => match.suggestionGroupId === groupId && !match.isHidden
    );
  }
  /**
   * @returns The list of side types to show.
   */
  sideTypes_() {
    return this.showSecondarySide_ ? [SideType.kDefaultPrimary, SideType.kSecondary] : [SideType.kDefaultPrimary];
  }
  /**
   * @returns The side type for the given suggestion group ID.
   */
  sideTypeForGroup_(groupId) {
    return this.result?.suggestionGroupsMap[groupId]?.sideType ?? SideType.kDefaultPrimary;
  }
  computeShowSecondarySide_() {
    if (!this.canShowSecondarySide) {
      return false;
    }
    const primaryGroupIds = this.groupIdsForSideType_(SideType.kDefaultPrimary);
    return primaryGroupIds.some((groupId) => {
      return this.matchesForGroup_(groupId).length > 0;
    });
  }
};
customElements.define(SearchboxDropdownElement.is, SearchboxDropdownElement);
var KeywordModeManager = class {
  keywordSpaceTriggeringEnabled = loadTimeData.isInitialized() && loadTimeData.valueExists("keywordSpaceTriggeringEnabled") ? loadTimeData.getBoolean("keywordSpaceTriggeringEnabled") : true;
  availableKeywordModels_ = /* @__PURE__ */ new Map();
  inputKeywordModel_ = null;
  entryMethod_ = 0;
  delegate_;
  constructor(delegate) {
    this.delegate_ = delegate;
  }
  get entryMethod() {
    return this.entryMethod_;
  }
  get availableKeywordModels() {
    return Array.from(this.availableKeywordModels_.values());
  }
  set availableKeywordModels(models) {
    this.availableKeywordModels_ = new Map(models.map((model) => [model.keyword.toLowerCase(), model]));
  }
  get inputKeywordModel() {
    return this.inputKeywordModel_;
  }
  set inputKeywordModel(model) {
    this.inputKeywordModel_ = model;
    this.delegate_.onKeywordModelChanged();
  }
  get isInKeywordMode() {
    return this.inputKeywordModel_?.type === KeywordType.kInKeyword;
  }
  get activeKeyword() {
    return this.isInKeywordMode && this.inputKeywordModel_?.keyword ? this.inputKeywordModel_.keyword : "";
  }
  /**
   * Enters keyword mode with the specified keyword, displayText hint, and entry
   * method.
   */
  enter(keyword, displayText, entryMethod) {
    this.entryMethod_ = entryMethod;
    this.inputKeywordModel = {
      type: KeywordType.kInKeyword,
      keyword,
      displayText
    };
  }
  /**
   * Exits keyword mode, resetting the keyword model and entry state.
   */
  exit() {
    this.entryMethod_ = 0;
    this.inputKeywordModel = null;
  }
  /**
   * Handles Backspace when cursor is at index 0 in keyword mode, exiting
   * keyword mode and notifying the delegate of the restored text and cursor.
   * Returns true if Backspace was handled.
   */
  handleBackspace(inputState) {
    const cursorAtStart = inputState.selectionStart === 0 && inputState.selectionEnd === 0;
    if (!this.isInKeywordMode || !cursorAtStart) {
      return false;
    }
    let prefix = this.activeKeyword ? `${this.activeKeyword} ` : "";
    if (this.entryMethod_ === 1 && !inputState.value) {
      prefix = this.activeKeyword;
    } else if (this.entryMethod_ === 3) {
      prefix = "?";
    } else if (this.entryMethod_ === 4) {
      prefix = "";
    }
    const restoredText = prefix + inputState.value;
    const newCursorPos = prefix.length;
    this.exit();
    this.delegate_.onKeywordCleared({
      restoredText,
      cursorPosition: newCursorPos
    });
    return true;
  }
  /**
   * Handles clicking a keyword chip on an autocomplete match, entering keyword
   * mode and notifying the delegate.
   */
  handleKeywordClick(match) {
    assert(match.keywordModel);
    this.enter(
      match.keywordModel.keyword,
      match.keywordModel.chipHint,
      5
      /* CLICK */
    );
    this.delegate_.onKeywordEntered();
  }
  /**
   * Evaluates whether pressing Tab on the selected match triggers keyword mode.
   * Only default matches (matchIndex === 0 and allowedToBeDefaultMatch) can
   * accept keyword mode via Tab. If triggered, enters keyword mode, notifies
   * the delegate, and returns true.
   */
  acceptTab(match, matchIndex) {
    if (!match?.keywordModel) {
      return false;
    }
    const isDefaultMatch = matchIndex === 0 && match.allowedToBeDefaultMatch;
    if (!isDefaultMatch) {
      return false;
    }
    this.enter(
      match.keywordModel.keyword,
      match.keywordModel.chipHint,
      1
      /* TAB */
    );
    this.delegate_.onKeywordEntered();
    return true;
  }
  /**
   * Evaluates whether the updated input text and cursor position trigger
   * keyword mode (e.g. space after instant keyword, or leading '?').
   * If triggered, enters keyword mode and returns true.
   */
  acceptInputTrigger(input, cursorPosition, event) {
    if (cursorPosition === null) {
      return false;
    }
    return this.acceptSpaceAtEnd_(input, cursorPosition, event) || this.acceptSpaceInMiddle_(input, cursorPosition, event) || this.acceptQuestionMark_(input, cursorPosition);
  }
  isSpaceEvent_(event) {
    if (event instanceof KeyboardEvent) {
      return event.key === " " || event.key === "\u3000";
    }
    if (event instanceof InputEvent) {
      return (event.data === " " || event.data === "\u3000") && event.inputType !== "insertFromPaste";
    }
    return false;
  }
  acceptSpaceAtEnd_(input, cursorPosition, event) {
    if (!this.keywordSpaceTriggeringEnabled) {
      return false;
    }
    if (this.isInKeywordMode) {
      return false;
    }
    if (!this.isSpaceEvent_(event)) {
      return false;
    }
    if (cursorPosition !== input.length) {
      return false;
    }
    if (!input.endsWith(" ") && !input.endsWith("\u3000")) {
      return false;
    }
    const candidate = input.slice(0, -1);
    if (!candidate || candidate.includes(" ") || candidate.includes("\u3000")) {
      return false;
    }
    const lowerCandidate = candidate.toLowerCase();
    const model = this.availableKeywordModels_.get(lowerCandidate) || (this.inputKeywordModel_?.keyword.toLowerCase() === lowerCandidate ? this.inputKeywordModel_ : null);
    if (!model) {
      return false;
    }
    const keyword = model.keyword;
    const displayText = model.displayText || keyword;
    this.enter(
      keyword,
      displayText,
      2
      /* SPACE_AT_END */
    );
    return true;
  }
  acceptSpaceInMiddle_(input, cursorPosition, event) {
    if (!this.keywordSpaceTriggeringEnabled) {
      return false;
    }
    if (this.isInKeywordMode) {
      return false;
    }
    if (!this.isSpaceEvent_(event)) {
      return false;
    }
    const spacePosition = cursorPosition - 1;
    if (spacePosition <= 0 || cursorPosition >= input.length) {
      return false;
    }
    const spaceChar = input[spacePosition];
    if (spaceChar !== " " && spaceChar !== "\u3000") {
      return false;
    }
    const charBeforeSpace = input[spacePosition - 1];
    if (charBeforeSpace === " " || charBeforeSpace === "\u3000") {
      return false;
    }
    const candidate = input.slice(0, spacePosition);
    if (candidate.includes(" ") || candidate.includes("\u3000")) {
      return false;
    }
    const lowerCandidate = candidate.toLowerCase();
    const model = this.availableKeywordModels_.get(lowerCandidate) || (this.inputKeywordModel_?.keyword.toLowerCase() === lowerCandidate ? this.inputKeywordModel_ : null);
    if (!model) {
      return false;
    }
    const textAfter = input.slice(cursorPosition);
    if (!textAfter.trim() || textAfter.startsWith(" ") || textAfter.startsWith("\u3000")) {
      return false;
    }
    const keyword = model.keyword;
    const displayText = model.displayText || keyword;
    this.enter(
      keyword,
      displayText,
      6
      /* SPACE_IN_MIDDLE */
    );
    return true;
  }
  acceptQuestionMark_(input, cursorPosition) {
    if (cursorPosition !== 1) {
      return false;
    }
    if (input !== "?") {
      return false;
    }
    if (this.isInKeywordMode) {
      return false;
    }
    this.enter(
      "?",
      "",
      3
      /* QUESTION_MARK */
    );
    return true;
  }
  /**
   * Formats a match's fillIntoEdit string when in keyword mode (stripping the
   * redundant keyword prefix), or restores the original input text for default
   * matches (avoiding prepending URL schemes like https://).
   */
  formatMatchFillIntoEdit(match, matchIndex, lastQueriedInput) {
    if (this.isInKeywordMode) {
      const keyword = this.inputKeywordModel_?.keyword;
      if (keyword) {
        const lowerKeyword = keyword.toLowerCase();
        const lowerFill = match.fillIntoEdit.toLowerCase();
        if (lowerFill.startsWith(lowerKeyword + " ")) {
          return match.fillIntoEdit.substring(keyword.length + 1);
        }
        if (lowerFill === lowerKeyword || match.keywordModel?.type !== KeywordType.kInKeyword && match.keywordModel?.keyword.toLowerCase() === lowerKeyword) {
          return "";
        }
      }
      return match.fillIntoEdit;
    }
    const isDefaultMatch = matchIndex === 0 && match.allowedToBeDefaultMatch;
    if (isDefaultMatch && lastQueriedInput) {
      return lastQueriedInput + match.inlineAutocompletion;
    }
    return match.fillIntoEdit;
  }
  /**
   * Updates or preserves the keyword model when the selected autocomplete match
   * or popup selection changes.
   */
  onSelectedMatchChanged(selectedMatch, selection) {
    if (!selectedMatch) {
      if (!this.isInKeywordMode) {
        this.inputKeywordModel = null;
      }
      return;
    }
    const isKeywordChipSelected = selection?.state === SelectionLineState.kKeywordMode || selectedMatch.keywordModel?.type === KeywordType.kInstant;
    if (isKeywordChipSelected && selectedMatch.keywordModel) {
      if (!this.isInKeywordMode || this.activeKeyword.toLowerCase() !== selectedMatch.keywordModel.keyword.toLowerCase()) {
        this.enter(
          selectedMatch.keywordModel.keyword,
          selectedMatch.keywordModel.chipHint,
          1
          /* TAB */
        );
      }
      return;
    }
    if (selectedMatch.keywordModel?.type === KeywordType.kInKeyword) {
      return;
    }
    if (this.isInKeywordMode) {
      this.exit();
    }
    if (!selectedMatch.keywordModel) {
      this.inputKeywordModel = null;
      return;
    }
    this.inputKeywordModel = {
      type: selectedMatch.keywordModel.type,
      keyword: selectedMatch.keywordModel.keyword,
      displayText: selectedMatch.keywordModel.chipHint
    };
  }
};
var SearchboxMixin = (superClass) => {
  class SearchboxMixin2 extends SearchboxSelectionMixin(superClass) {
    #virtualFocusEnabled = false;
    get virtualFocusEnabled() {
      return this.#virtualFocusEnabled;
    }
    set virtualFocusEnabled(_2) {
      this.#virtualFocusEnabled = _2;
    }
    static get properties() {
      return {
        virtualFocusEnabled: {
          type: Boolean
        },
        dropdownIsVisible: {
          type: Boolean,
          reflect: true
        },
        /** The value of the input element's 'aria-live' attribute. */
        inputAriaLive: {
          type: String
        },
        multiLineEnabled: {
          type: Boolean,
          reflect: true
        },
        result: {
          type: Object
        },
        selectedMatch: {
          type: Object
        },
        selectedMatchIndex: {
          type: Number
        },
        inputKeywordModel: {
          type: Object
        },
        /** The aria description to include on the input element. */
        searchboxAriaDescription: {
          type: String
        },
        /** Searchbox default icon (i.e., Google G icon or the search loupe). */
        searchboxIcon: {
          type: String
        },
        showThumbnail: {
          type: Boolean,
          reflect: true
        }
      };
    }
    composeboxSource = loadTimeData.valueExists("composeboxSource") ? loadTimeData.getString("composeboxSource") : "Unknown";
    #searchboxAriaDescription = "";
    get searchboxAriaDescription() {
      return this.#searchboxAriaDescription;
    }
    set searchboxAriaDescription(_2) {
      this.#searchboxAriaDescription = _2;
    }
    #dropdownIsVisible = false;
    get dropdownIsVisible() {
      return this.#dropdownIsVisible;
    }
    set dropdownIsVisible(_2) {
      this.#dropdownIsVisible = _2;
    }
    // Tracks the latest query sent for autocompletion. Used to filter out
    // stale results. `activeQueryId` is reset to -1 when the last query needs
    // to be abandoned. `nextQueryId_` is monotonically increasing to avoid
    // reusing IDs.
    activeQueryId = -1;
    nextQueryId_ = 0;
    #lastQueriedInput = null;
    get lastQueriedInput() {
      return this.#lastQueriedInput;
    }
    set lastQueriedInput(_2) {
      this.#lastQueriedInput = _2;
    }
    #multiLineEnabled = false;
    get multiLineEnabled() {
      return this.#multiLineEnabled;
    }
    set multiLineEnabled(_2) {
      this.#multiLineEnabled = _2;
    }
    #result = null;
    get result() {
      return this.#result;
    }
    set result(_2) {
      this.#result = _2;
    }
    #selectedMatch = null;
    get selectedMatch() {
      return this.#selectedMatch;
    }
    set selectedMatch(_2) {
      this.#selectedMatch = _2;
    }
    #selectedMatchIndex = -1;
    get selectedMatchIndex() {
      return this.#selectedMatchIndex;
    }
    set selectedMatchIndex(_2) {
      this.#selectedMatchIndex = _2;
    }
    get matchIndex() {
      if (this.virtualFocusEnabled) {
        if (this.selection.line >= 0) {
          return this.selection.line;
        }
        return this.result?.matches?.[0]?.allowedToBeDefaultMatch ? 0 : -1;
      }
      if (this.selectedMatchIndex >= 0) {
        return this.selectedMatchIndex;
      }
      return this.result?.matches?.[0]?.allowedToBeDefaultMatch ? 0 : -1;
    }
    #inputAriaLive = "";
    get inputAriaLive() {
      return this.#inputAriaLive;
    }
    set inputAriaLive(_2) {
      this.#inputAriaLive = _2;
    }
    #searchboxIcon = "";
    get searchboxIcon() {
      return this.#searchboxIcon;
    }
    set searchboxIcon(_2) {
      this.#searchboxIcon = _2;
    }
    #showThumbnail = false;
    get showThumbnail() {
      return this.#showThumbnail;
    }
    set showThumbnail(_2) {
      this.#showThumbnail = _2;
    }
    keywordModeManager_ = new KeywordModeManager({
      onKeywordModelChanged: () => {
        this.requestUpdate("inputKeywordModel");
      },
      onKeywordCleared: (event) => {
        this.getInputElement().setInput({
          text: event.restoredText,
          inline: "",
          moveCursorToEnd: false
        });
        this.getInputElement().inputElement?.setSelectionRange(
          event.cursorPosition,
          event.cursorPosition
        );
        this.queryAutocomplete(
          event.restoredText,
          /*preventInlineAutocomplete=*/
          true,
          /*isOnFocus=*/
          false
        );
      },
      onKeywordEntered: () => {
        this.getInputElement().setInputText("");
      }
    });
    get inputKeywordModel() {
      return this.keywordModeManager_.inputKeywordModel;
    }
    set inputKeywordModel(model) {
      this.keywordModeManager_.inputKeywordModel = model;
    }
    get keywordModeManager() {
      return this.keywordModeManager_;
    }
    initialInputScrollHeight = 0;
    controlKeyState_ = 0;
    lastIgnoredEnterEvent_ = null;
    searchboxEventTracker_ = new EventTracker();
    callbackRouter_ = SearchboxBrowserProxy.getInstance().callbackRouter;
    keywordSpaceTriggeringListenerId_ = null;
    availableKeywordModelsListenerId_ = null;
    connectedCallback() {
      super.connectedCallback();
      this.keywordSpaceTriggeringListenerId_ = this.callbackRouter_.setKeywordSpaceTriggeringEnabled.addListener(
        (enabled) => {
          this.keywordModeManager_.keywordSpaceTriggeringEnabled = enabled;
        }
      );
      this.availableKeywordModelsListenerId_ = this.callbackRouter_.setAvailableKeywordModels.addListener(
        (models) => {
          this.keywordModeManager_.availableKeywordModels = models;
        }
      );
      this.searchboxEventTracker_.add(this, "input-mousedown", () => {
        this.activeQueryId = -1;
      });
      this.searchboxEventTracker_.add(this, "match-remove", () => {
        this.activeQueryId = this.nextQueryId_ - 1;
      });
      this.searchboxEventTracker_.add(window, "keyup", (e5) => {
        if (this.shouldAppendDotComOnCtrlEnter() && e5.key === "Control") {
          this.controlKeyState_ = 0;
        }
      });
    }
    disconnectedCallback() {
      super.disconnectedCallback();
      this.searchboxEventTracker_.removeAll();
      if (this.keywordSpaceTriggeringListenerId_ !== null) {
        this.callbackRouter_.removeListener(
          this.keywordSpaceTriggeringListenerId_
        );
        this.keywordSpaceTriggeringListenerId_ = null;
      }
      if (this.availableKeywordModelsListenerId_ !== null) {
        this.callbackRouter_.removeListener(
          this.availableKeywordModelsListenerId_
        );
        this.availableKeywordModelsListenerId_ = null;
      }
    }
    willUpdate(changedProperties) {
      super.willUpdate(changedProperties);
      const changedPrivateProperties = changedProperties;
      if (changedPrivateProperties.has("selectedMatch")) {
        this.inputAriaLive = this.computeInputAriaLive_();
      }
      if (changedPrivateProperties.has("result") || changedPrivateProperties.has("selectedMatchIndex") || changedPrivateProperties.has("selection")) {
        this.selectedMatch = this.computeSelectedMatch_();
      }
      if (changedPrivateProperties.has("result") || changedPrivateProperties.has("selectedMatchIndex") || changedPrivateProperties.has("selectedMatch") || changedPrivateProperties.has("selection")) {
        this.keywordModeManager_.onSelectedMatchChanged(
          this.selectedMatch,
          this.selection
        );
      }
    }
    updated(changedProperties) {
      super.updated(changedProperties);
      const changedPrivateProperties = changedProperties;
      if (changedPrivateProperties.has("showThumbnail")) {
        const dropdown = this.getDropdownElement();
        if (dropdown) {
          dropdown.showThumbnail = this.showThumbnail;
        }
      }
    }
    getInputElement() {
      assertNotReached();
    }
    getDropdownElement() {
      assertNotReached();
    }
    getWrapperElement() {
      assertNotReached();
    }
    getTabId() {
      return null;
    }
    pageHandler() {
      assertNotReached();
    }
    /**
     * Clears the autocomplete result on the page and on the autocomplete
     * backend.
     */
    clearAutocompleteMatches() {
      this.dropdownIsVisible = false;
      this.result = null;
      this.getDropdownElement().unselect();
      this.pageHandler().stopAutocomplete(
        /*clearResult=*/
        true
      );
      this.activeQueryId = -1;
      this.lastQueriedInput = null;
    }
    queryAutocomplete(input, preventInlineAutocomplete, isOnFocus) {
      this.activeQueryId = this.nextQueryId_++;
      this.lastQueriedInput = input;
      preventInlineAutocomplete = preventInlineAutocomplete || this.getInputElement().preventInlineAutocomplete(input);
      const cursorPosition = this.getInputElement().inputElement.value === input ? this.getInputElement().inputElement.selectionStart || 0 : input.length;
      const keyword = this.keywordModeManager_.activeKeyword;
      this.pageHandler().queryAutocomplete(
        this.activeQueryId,
        this.getTabId(),
        input,
        preventInlineAutocomplete,
        cursorPosition,
        SuggestInventory.kDefault,
        isOnFocus,
        keyword,
        InputMethod.kKeyboard
      );
      this.dispatchEvent(new CustomEvent("query-autocomplete", {
        bubbles: true,
        composed: true,
        detail: { inputValue: input }
      }));
    }
    shouldAppendDotComOnCtrlEnter() {
      return false;
    }
    isBackgroundTabNavigation(_e) {
      return false;
    }
    navigateToMatch(matchIndex, e5) {
      assert(matchIndex >= 0);
      const match = this.result.matches[matchIndex];
      assert(match);
      this.pageHandler().openAutocompleteMatch(
        matchIndex,
        match.destinationUrl,
        /*areMatchesShowing=*/
        this.dropdownIsVisible,
        /*mouseButton=*/
        e5.button || 0,
        {
          altKey: e5.altKey,
          ctrlKey: e5.ctrlKey,
          metaKey: e5.metaKey,
          shiftKey: e5.shiftKey
        },
        /*viaKeyboard=*/
        e5 instanceof KeyboardEvent
      );
      const isBackgroundTab = this.isBackgroundTabNavigation(e5);
      if (!isBackgroundTab) {
        const fillText = this.keywordModeManager_.formatMatchFillIntoEdit(
          match,
          matchIndex,
          this.lastQueriedInput
        );
        this.getInputElement().setInput({
          text: fillText,
          inline: "",
          moveCursorToEnd: true
        });
        this.clearAutocompleteMatches();
      }
      e5.preventDefault();
    }
    openCtrlEnterMatch(matchIndex) {
      assert(matchIndex >= 0);
      const match = this.result.matches[matchIndex];
      assert(match);
      this.pageHandler().openPopupSelection(
        this.result.sequenceId,
        {
          line: matchIndex,
          state: SelectionLineState.kCtrlEnter,
          actionIndex: 0
        },
        1
      );
      this.getInputElement().setInput({
        text: match.fillIntoEdit,
        inline: "",
        moveCursorToEnd: true
      });
      this.clearAutocompleteMatches();
    }
    openContextMenu() {
    }
    isAutocompleteResultStale(result) {
      return result.queryId !== this.activeQueryId;
    }
    updateDropdownVisibility() {
      this.dropdownIsVisible = this.hasMatches();
    }
    async onAutocompleteResultChanged(result) {
      if (this.isAutocompleteResultStale(result)) {
        return;
      }
      this.result = result;
      const hasMatches = this.hasMatches();
      this.updateDropdownVisibility();
      const firstMatch = hasMatches ? this.result.matches[0] : null;
      if (firstMatch && firstMatch.allowedToBeDefaultMatch) {
        if (this.virtualFocusEnabled) {
          const available = this.getAvailableSelections(this.result);
          this.setSelection(available[0] || kDefaultSelection);
        } else {
          this.getDropdownElement().selectFirst();
        }
        this.getInputElement().setInput({
          text: this.lastQueriedInput ?? "",
          inline: firstMatch.inlineAutocompletion
        });
        if (this.lastIgnoredEnterEvent_) {
          this.navigateToMatch(0, this.lastIgnoredEnterEvent_);
          this.lastIgnoredEnterEvent_ = null;
        }
      } else {
        const index = this.matchIndex;
        if (this.getInputElement().inputElement.value.trim() && hasMatches && index >= 0 && index < this.result.matches.length) {
          const match = this.result.matches[index];
          this.selectedMatch = match;
          if (this.virtualFocusEnabled) {
            this.setSelection({
              line: index,
              state: SelectionLineState.kNormal,
              actionIndex: 0
            });
          }
          await this.getDropdownElement().selectIndex(index);
          this.getInputElement().setInput({
            text: this.computeMatchFillIntoEdit(match),
            inline: "",
            moveCursorToEnd: true
          });
        } else {
          this.getDropdownElement().unselect();
          this.getInputElement().setInput({
            inline: ""
          });
        }
      }
    }
    onInputFocusChanged(e5) {
      if (this.shouldAppendDotComOnCtrlEnter()) {
        this.controlKeyState_ = 0;
      }
      if (this.dropdownIsVisible) {
        return;
      }
      const input = e5.detail.value;
      const isOnFocus = e5.detail.isOnFocus;
      this.queryAutocomplete(
        input,
        /*preventInlineAutocomplete=*/
        false,
        isOnFocus
      );
    }
    onSearchboxInputTextUpdated(e5) {
      const input = e5.detail.value;
      const cursorPosition = this.getInputElement().inputElement?.selectionStart ?? null;
      const event = e5.detail.event ?? null;
      if (this.keywordModeManager_.acceptInputTrigger(
        input,
        cursorPosition,
        event
      )) {
        const isSpaceInMiddle = this.keywordModeManager_.entryMethod === 6;
        const remainingText = isSpaceInMiddle ? input.slice(
          cursorPosition ?? this.keywordModeManager_.activeKeyword.length + 1
        ) : "";
        this.getInputElement().setInputText(remainingText);
        if (isSpaceInMiddle) {
          this.getInputElement().setSelectionRange(0, 0);
        }
        this.queryAutocomplete(
          remainingText,
          /*preventInlineAutocomplete=*/
          false,
          /*isOnFocus=*/
          false
        );
        return;
      }
      const isEmpty = !input.trim() && !this.keywordModeManager_.isInKeywordMode;
      if (isEmpty) {
        this.clearAutocompleteMatches();
      } else {
        this.queryAutocomplete(
          input,
          /*preventInlineAutocomplete=*/
          e5.detail.isComposing,
          /*isOnFocus=*/
          false
        );
      }
    }
    onInputWrapperFocusout(e5) {
      const newlyFocusedEl = e5.relatedTarget;
      if (this.getWrapperElement().contains(newlyFocusedEl)) {
        return;
      }
      if (this.lastQueriedInput === "" && !this.keywordModeManager_.isInKeywordMode) {
        this.getInputElement().setInput({ text: "", inline: "" });
        this.clearAutocompleteMatches();
      } else {
        this.dropdownIsVisible = false;
        this.activeQueryId = -1;
        this.pageHandler().stopAutocomplete(
          /*clearResult=*/
          false
        );
      }
      this.pageHandler().onFocusChanged(false);
    }
    async onInputWrapperKeydown(e5) {
      if (e5.key !== "Enter") {
        this.activeQueryId = -1;
      }
      const modifier = isMac ? e5.metaKey && !e5.ctrlKey : e5.ctrlKey && !e5.metaKey;
      if (modifier && e5.key === "z") {
        e5.stopPropagation();
        return;
      }
      if (this.shouldAppendDotComOnCtrlEnter()) {
        if (e5.key === "Control") {
          if (this.controlKeyState_ === 0) {
            this.controlKeyState_ = 1;
          }
        } else if (e5.ctrlKey && e5.key !== "Enter") {
          if (this.controlKeyState_ === 1) {
            this.controlKeyState_ = 2;
          }
        }
      }
      const KEYDOWN_HANDLED_KEYS = [
        "ArrowDown",
        "ArrowUp",
        "Backspace",
        "Delete",
        "Enter",
        "Escape",
        "PageDown",
        "PageUp",
        "Tab"
      ];
      if (!KEYDOWN_HANDLED_KEYS.includes(e5.key)) {
        return;
      }
      if (e5.defaultPrevented) {
        return;
      }
      await this.handleKeyNavigation(e5);
    }
    hasMatches() {
      return !!this.result && !!this.result.matches && this.result.matches.length > 0;
    }
    /**
     * Determines whether the key event originated from an element participating
     * in virtual focus navigation (the input, dropdown matches, or compose
     * button). Events from nested controls (e.g. contextual entrypoints, lens
     * button, voice search) return false so native browser Tab navigation
     * applies.
     */
    isVirtualFocusEventTarget_(e5) {
      const path = e5.composedPath();
      if (path.length === 0) {
        return true;
      }
      return path.includes(this.getInputElement()) || path.includes(this.getDropdownElement()) || path.some((el) => {
        const node = el;
        return node.tagName === "CR-SEARCHBOX-COMPOSE-BUTTON";
      });
    }
    /**
     * Handles Enter key presses on virtually focused elements (AIM, Action,
     * Remove Suggestion). Returns true if the event was handled.
     */
    handleVirtualFocusEnter_(e5) {
      if (this.selection.state === SelectionLineState.kFocusedButtonAim) {
        e5.preventDefault();
        const button = this.shadowRoot.querySelector("cr-searchbox-compose-button");
        if (button) {
          button.dispatchEvent(new CustomEvent("compose-click", {
            bubbles: true,
            composed: true,
            detail: {
              button: 0,
              ctrlKey: e5.ctrlKey,
              metaKey: e5.metaKey,
              shiftKey: e5.shiftKey
            }
          }));
        }
        return true;
      }
      if (this.selection.state === SelectionLineState.kFocusedButtonAction) {
        e5.preventDefault();
        const action = this.selectedMatch?.actions[this.selection.actionIndex];
        if (action) {
          this.pageHandler().executeAction(
            this.selection.line,
            this.selection.actionIndex,
            this.selectedMatch.destinationUrl,
            mojoTimeTicks(Date.now()),
            0,
            e5.altKey,
            e5.ctrlKey,
            e5.metaKey,
            e5.shiftKey
          );
        }
        return true;
      }
      if (this.selection.state === SelectionLineState.kFocusedButtonRemoveSuggestion) {
        e5.preventDefault();
        if (this.selectedMatch && this.selectedMatch.supportsDeletion) {
          this.unfreezeActiveQueryId();
          this.pageHandler().deleteAutocompleteMatch(
            this.selection.line,
            this.selectedMatch.destinationUrl
          );
        }
        return true;
      }
      if (this.selection.state === SelectionLineState.kFocusedButtonContextEntrypoint) {
        e5.preventDefault();
        this.openContextMenu();
        return true;
      }
      if (this.selection.state === SelectionLineState.kKeywordMode) {
        e5.preventDefault();
        this.getInputElement().focus();
        return true;
      }
      return false;
    }
    updateInputForSelection_(nextSelection, key) {
      if (this.selectedMatch) {
        const newFill = this.computeMatchFillIntoEdit(this.selectedMatch);
        const isKeywordMode = this.keywordModeManager_.isInKeywordMode || nextSelection.state === SelectionLineState.kKeywordMode;
        const newInline = !isKeywordMode && nextSelection.line === 0 && this.selectedMatch.allowedToBeDefaultMatch ? this.selectedMatch.inlineAutocompletion : "";
        const newFillEnd = newFill.length - newInline.length;
        const text = newFill.substr(0, newFillEnd);
        this.getInputElement().setInput({
          text,
          inline: newInline,
          moveCursorToEnd: newInline.length === 0
        });
        if (key === "ArrowDown" || key === "ArrowUp") {
          this.pageHandler().onNavigationLikely(
            nextSelection.line,
            this.selectedMatch.destinationUrl,
            NavigationPredictor.kUpOrDownArrowButton
          );
        }
      } else if (nextSelection.line === -1) {
        this.getInputElement().setInput({
          text: this.lastQueriedInput ?? "",
          inline: "",
          moveCursorToEnd: true
        });
      }
    }
    handleEnterNavigation_(e5) {
      if (this.multiLineEnabled && e5.shiftKey) {
        return;
      }
      const isPureCtrlEnter = this.shouldAppendDotComOnCtrlEnter() && e5.ctrlKey && !e5.shiftKey && !e5.altKey && !e5.metaKey && this.controlKeyState_ !== 2;
      e5.preventDefault();
      if (this.handleVirtualFocusEnter_(e5)) {
        return;
      }
      if (this.activeQueryId === -1 || this.result?.queryId === this.activeQueryId) {
        if (this.selectedMatch) {
          if (isPureCtrlEnter) {
            this.openCtrlEnterMatch(this.matchIndex);
          } else {
            this.navigateToMatch(this.matchIndex, e5);
          }
        }
      } else {
        this.lastIgnoredEnterEvent_ = e5;
        this.activeQueryId = this.nextQueryId_ - 1;
      }
    }
    async handleKeyNavigation(e5) {
      if (e5.key === "Backspace") {
        const inputEl = this.getInputElement().inputElement;
        if (inputEl && this.keywordModeManager_.handleBackspace(inputEl)) {
          e5.preventDefault();
        }
        return;
      }
      if (e5.key === "Tab") {
        if (!this.virtualFocusEnabled && !e5.shiftKey && !e5.isComposing && this.keywordModeManager_.acceptTab(
          this.selectedMatch,
          this.matchIndex
        )) {
          e5.preventDefault();
          return;
        }
        if (!this.virtualFocusEnabled || !this.dropdownIsVisible || !this.isVirtualFocusEventTarget_(e5)) {
          return;
        }
      }
      if (!this.dropdownIsVisible) {
        if (e5.key === "ArrowUp" || e5.key === "ArrowDown") {
          const inputValue = this.getInputElement().inputElement.value;
          if (inputValue.trim() || !inputValue) {
            this.queryAutocomplete(
              inputValue,
              /*preventInlineAutocomplete=*/
              false,
              /*isOnFocus=*/
              !inputValue
            );
          }
          e5.preventDefault();
          return;
        }
      }
      if (e5.key === "Escape") {
        this.dispatchEvent(new CustomEvent("escape-searchbox", {
          bubbles: true,
          composed: true,
          detail: {
            event: e5,
            emptyInput: !this.getInputElement().inputElement.value
          }
        }));
      }
      if (e5.isComposing) {
        return;
      }
      if (this.virtualFocusEnabled && e5.key === "Enter" && this.handleVirtualFocusEnter_(e5)) {
        return;
      }
      if (!this.result || this.result.matches.length === 0) {
        return;
      }
      if (e5.key === "Delete") {
        if (e5.shiftKey && !e5.altKey && !e5.ctrlKey && !e5.metaKey) {
          if (this.selectedMatch && this.selectedMatch.supportsDeletion) {
            this.activeQueryId = this.nextQueryId_ - 1;
            this.pageHandler().deleteAutocompleteMatch(
              this.selectedMatchIndex,
              this.selectedMatch.destinationUrl
            );
            e5.preventDefault();
          }
        }
        return;
      }
      if (this.virtualFocusEnabled) {
        let step = SelectionStep.kStateOrLine;
        let direction = SelectionDirection.kForward;
        let valid = false;
        if (!e5.altKey && !e5.ctrlKey && !e5.metaKey) {
          if (e5.key === "Tab" && this.dropdownIsVisible) {
            step = SelectionStep.kStateOrLine;
            direction = e5.shiftKey ? SelectionDirection.kBackward : SelectionDirection.kForward;
            valid = true;
          } else if (!e5.shiftKey) {
            if (e5.key === "ArrowDown") {
              step = SelectionStep.kWholeLine;
              direction = SelectionDirection.kForward;
              valid = true;
            } else if (e5.key === "ArrowUp") {
              step = SelectionStep.kWholeLine;
              direction = SelectionDirection.kBackward;
              valid = true;
            } else if (e5.key === "PageDown") {
              step = SelectionStep.kAllLines;
              direction = SelectionDirection.kForward;
              valid = true;
            } else if (e5.key === "PageUp" || e5.key === "Escape") {
              step = SelectionStep.kAllLines;
              direction = SelectionDirection.kBackward;
              valid = true;
            }
          }
        }
        if (valid) {
          if (e5.key === "Tab") {
            if (this.stepCyclesSelection(
              this.result,
              this.selection,
              direction,
              step
            )) {
              this.setSelection(kDefaultSelection);
              return;
            }
          }
          const nextSelection = this.getNextSelection(
            this.result,
            this.selection,
            direction,
            step
          );
          if (selectionsEqual(nextSelection, this.selection)) {
            if (e5.key === "Escape") {
              this.getInputElement().setInput({ text: "", inline: "" });
              this.clearAutocompleteMatches();
              e5.preventDefault();
            }
            return;
          }
          e5.preventDefault();
          this.setSelection(nextSelection);
          this.getInputElement().focus();
          await this.updateComplete;
          this.updateInputForSelection_(nextSelection, e5.key);
          return;
        }
      }
      if (e5.key === "Enter") {
        this.handleEnterNavigation_(e5);
        return;
      }
      if (hasKeyModifiers(e5)) {
        return;
      }
      if (e5.key === "Escape" && this.selectedMatchIndex <= 0) {
        this.getInputElement().setInput({ text: "", inline: "" });
        this.clearAutocompleteMatches();
        e5.preventDefault();
        return;
      }
      e5.preventDefault();
      if (e5.key === "ArrowDown") {
        await this.getDropdownElement().selectNext();
      } else if (e5.key === "ArrowUp") {
        await this.getDropdownElement().selectPrevious();
      } else if (e5.key === "Escape" || e5.key === "PageUp") {
        await this.getDropdownElement().selectFirst();
      } else if (e5.key === "PageDown") {
        await this.getDropdownElement().selectLast();
      }
      await this.updateComplete;
      if (e5.key === "ArrowDown" || e5.key === "ArrowUp") {
        if (this.selectedMatch) {
          this.pageHandler().onNavigationLikely(
            this.selectedMatchIndex,
            this.selectedMatch.destinationUrl,
            NavigationPredictor.kUpOrDownArrowButton
          );
        }
      }
      if (this.shadowRoot.activeElement === this.getDropdownElement()) {
        this.getDropdownElement().focusSelected();
      }
      if (this.selectedMatch) {
        const newFill = this.computeMatchFillIntoEdit(this.selectedMatch);
        const newInline = this.selectedMatchIndex === 0 && this.selectedMatch.allowedToBeDefaultMatch ? this.selectedMatch.inlineAutocompletion : "";
        const newFillEnd = newFill.length - newInline.length;
        const text = newFill.substr(0, newFillEnd);
        if (!this.keywordModeManager_.isInKeywordMode) {
          assert(text);
        }
        this.getInputElement().setInput({
          text,
          inline: newInline,
          moveCursorToEnd: newInline.length === 0
        });
      }
    }
    onSelectedMatchIndexChanged(e5) {
      this.selectedMatchIndex = e5.detail.value;
    }
    onMatchClick() {
      this.clearAutocompleteMatches();
    }
    async onMatchFocusin(e5) {
      await this.getDropdownElement().selectIndex(e5.detail);
      const input = this.getInputElement();
      assert(input);
      if (this.selectedMatch) {
        input.setInput({
          text: this.computeMatchFillIntoEdit(this.selectedMatch),
          inline: "",
          moveCursorToEnd: true
        });
      }
    }
    computeMatchFillIntoEdit(match) {
      return this.keywordModeManager_.formatMatchFillIntoEdit(
        match,
        this.matchIndex,
        this.lastQueriedInput
      );
    }
    async onKeywordClick(e5) {
      const detail = e5.detail;
      const match = detail.match;
      assert(match?.keywordModel);
      this.keywordModeManager_.handleKeywordClick(match);
      const matchIndex = detail.matchIndex ?? (this.result?.matches ? this.result.matches.indexOf(match) : 0);
      const selection = {
        line: matchIndex >= 0 ? matchIndex : 0,
        state: SelectionLineState.kKeywordMode,
        actionIndex: 0
      };
      this.setSelection(selection);
      await this.updateComplete;
      this.updateInputForSelection_(selection, "click");
      this.getInputElement().focus();
    }
    computeSelectedMatch_() {
      if (!this.result || !this.result.matches) {
        return null;
      }
      return this.result.matches[this.matchIndex] || null;
    }
    computeInputAriaLive_() {
      return this.selectedMatch ? "off" : "polite";
    }
    /**
     * Accepts the inline autocompletion by appending it to the input text and
     * moving the cursor to the end. Returns `true` if inline autocomplete was
     * handled, `false` otherwise.
     */
    acceptInlineAutocomplete(e5) {
      const input = this.getInputElement();
      const lastInput = input?.lastInput();
      if (!lastInput?.inline) {
        return false;
      }
      if (e5.shiftKey) {
        input.setInput({ inline: "" });
        return true;
      }
      const newText = lastInput.text + lastInput.inline;
      input.setInput({
        text: newText,
        inline: "",
        moveCursorToEnd: true
      });
      this.queryAutocomplete(
        newText,
        /*preventInlineAutocomplete=*/
        false,
        /*isOnFocus=*/
        false
      );
      e5.preventDefault();
      return true;
    }
    unfreezeActiveQueryId() {
      this.activeQueryId = this.nextQueryId_ - 1;
    }
  }
  return SearchboxMixin2;
};
function getCss27() {
  return [i(["/* Copyright 2026 The Chromium Authors\n * Use of this source code is governed by a BSD-style license that can be\n * found in the LICENSE file. */\n\n/* #css_wrapper_metadata_start\n * #type=style-lit\n * #scheme=relative\n * #css_wrapper_metadata_end */\n\n.context-menu-container {\n  align-items: center;\n  cursor: text;\n  display: flex;\n  gap: 4px;\n  margin-bottom: var(--contextual-entrypoint-and-carousel-context-menu-container-margin-bottom);\n  padding-inline-start: var(--contextual-entrypoint-and-carousel-context-menu-container-padding-inline-start, 4px);\n  flex-wrap: nowrap;\n}\n\n#contextEntrypoint {\n  padding-inline-start:\n      var(--contextual-entrypoint-and-carousel-context-entrypoint-padding-inline-start,\n          8px);\n  padding-inline-end:\n      var(--contextual-entrypoint-and-carousel-context-entrypoint-padding-inline-end,\n          8px);\n}\n\n.context-menu-container > * {\n  flex-shrink: 0;\n}\n\n.context-menu-container:has(.contextual-chip) {\n  padding-bottom: var(--contextual-entrypoint-and-carousel-contextual-chip-padding-bottom);\n}\n\n.carousel-divider {\n  border-radius: 100px;\n  border-top: 1px solid\n    var(--color-composebox-file-carousel-divider);\n  margin-inline-end: 16px;\n  margin-inline-start: var(--text-input-inline-start-spacing);\n  margin-top: var(--contextual-entrypoint-and-carousel-carousel-divider-margin-top, 20px);\n  margin-bottom: var(--contextual-entrypoint-and-carousel-carousel-divider-margin-bottom, 10px);\n}\n\n.upload-button {\n  --cr-icon-button-focus-outline-color:\n      var(--color-searchbox-results-icon-focused-outline);\n  --cr-icon-button-icon-size: 24px;\n  --cr-icon-button-hover-background-color:\n      var(--color-searchbox-results-background-hovered);\n  --cr-icon-button-size: 48px;\n  color: var(--color-composebox-upload-button);\n  display: flex;\n  flex-wrap: nowrap;\n}\n\n/*\n * Controls the opacity of the icons when the composebox is expanded/collapsed.\n */\n.icon-fade {\n  opacity: 0;\n  transition: var(--icon-exit-transition);\n}\n\n:host-context([expanding_]) .icon-fade {\n  opacity: 1;\n  transition: var(--icon-entry-transition);\n}\n\n:host-context([in-voice-search-mode]) .icon-fade {\n  opacity: 0;\n  transition: var(--icon-exit-transition);\n}\n\n.action-icon {\n  --cr-icon-button-focus-outline-color:\n      var(--color-searchbox-results-icon-focused-outline);\n  --cr-icon-button-hover-background-color:\n      var(--color-searchbox-results-background-hovered);\n  --cr-icon-button-margin-end: 0;\n  --cr-icon-button-size: 36px;\n}\n\n#carousel,\n#voiceSearchCarousel {\n  box-sizing: border-box;\n  margin-bottom: var(--context-non-top-carousel-bottom-margin, 8px);\n  margin-top: var(--context-non-top-carousel-top-margin, 4px);\n  /* Prefer using margin, but from legacy var name, we should be using inset. */\n  inset-inline-start: var(--context-carousel-inline-start-spacing, 50px);\n  position: relative;\n  width: calc(100% - var(--context-carousel-inline-start-spacing, 50px) -\n      var(--context-carousel-inline-end-spacing, 50px));\n}\n\n#voiceCarouselContainer {\n  /* Ensure that it takes up as much room as possible. */\n  align-self: stretch;\n  /* Render below the recording wave, which is order 1.\n   * Above the tool chips, which are order 3. */\n  order: 2;\n}\n\n/* Both top and bottom inner wrappers should participate in the normal flow. */\n.carousel-container-inner,\n#voiceCarouselContainerInner {\n  display: flex;\n  flex-direction: column;\n  flex-grow: 1;\n  width: 100%;\n}\n\n/* Shows on the bottom: */\n#voiceSearchCarousel {\n  /* Width is 100% - right padding - left padding - gap.\n   * Gap: used to add space between carousel and bottom action buttons.\n   * Right padding: defined as either the bottom action buttons\n   * in voice search's width/gap/inset-end, or the right padding hard coded\n   * into carousel, whichever is larger).\n   */\n  width: calc(100% - var(--context-carousel-inline-start-spacing, 50px) -\n      max(var(--bottom-actions-width-and-spacing),\n          var(--context-carousel-inline-end-spacing, 50px)) -\n          var(--voice-bottom-actions-gap));  /* Stop button/carousel gap. */\n\n  /* Space between voice recording wave and the file carousel\n   * (below the recording wave) set by\n   * '--context-non-top-carousel-top-margin' in '#carousel'.\n   */\n}\n\n/* Controls toolchips in voice search. */\n#voiceToolChipsContainer {\n  order: 3; /* Stays below the carousel/the recording wave. */\n  /* Space between voice recording wave and the tool chip\n   * (below recording wave); matches Google3.\n   */\n  padding-top: 14px;\n  /* 10px of padding from outer container + this = 14px of space\n   * below toolchip.\n   */\n  padding-bottom: 4px;\n}\n\n\n:host,\n#voiceCarouselContainer {\n  --voice-bottom-actions-gap: 8px;\n  --voice-bottom-actions-inset-inline-end: 12px;\n  --voice-search-submit-default-container-size: 40px;\n  --voice-search-button-height: var(--cr-composebox-submit-height,\n      var(--voice-search-submit-default-container-size));\n  --voice-search-button-width: var(--cr-composebox-submit-width,\n      var(--voice-search-submit-default-container-size));\n  --bottom-actions-width-and-spacing: calc(2 * var(--voice-search-button-width)\n      + var(--voice-bottom-actions-inset-inline-end)\n      + var(--voice-bottom-actions-gap));\n  --voice-bottom-actions-height: var(--cr-composebox-button-height,\n      var(--voice-search-submit-default-container-size));\n  --voice-bottom-actions-bottom: 12px;\n}\n\n/* Make it relative so it pushes the voice animation\n * down if it is on top.\n */\n#voiceSearchCarousel.top {\n  /* Space between voice recording wave and the file carousel\n   * (above the recording wave) set by\n   * '--context-non-top-carousel-bottom-margin' in '#carousel'.\n   */\n  position: relative;\n  top: 0;\n}\n\n/* Position absolute: make sure error scrim shows over voice search\n * animation and voice search component since composebox is hidden\n * in voice search mode. 'Absolute' is needed for scrim because\n * absolutely-positioned voice search/animation are first before the\n * scrim in the DOM. This means 'non-static' voice search would push the\n * scrim too far down if it were static.\n */\n:host([in-voice-search-mode]) #errorScrim {\n  inset: 0;\n  position: absolute;\n   /* To show above voice search now that it is not static. */\n  z-index: 3;\n}\n\n/* Completely hide composebox and let animated_glow.css determine\n * the size of the voice search container and overlay\n * composebox_voice_search.css handles the stop/submit buttons only.\n */\n:host([in-voice-search-mode]) #composebox {\n  block-size: fit-content;\n  display: none;\n}\n\n/* This shows over search animation, but make it not clickable.\n * Its submit/stop button children override 'pointer-events' to 'auto'.\n */\ncr-composebox-voice-search {\n  display: block;\n  pointer-events: none;\n  z-index: 2;\n}\n\n/* When listening, make voice search 'absolute' to have composebox take\n * height from parent, which gets it from 'animated_glow'. Otherwise,\n * make voice search 'static', and thus composebox's source of height\n * when not listening and in voice search mode (when permission prompt\n * is showing).\n */\n:host([in-voice-search-mode][is-listening]) cr-composebox-voice-search {\n  inset: 0;\n  position: absolute;\n}\n\n:host(:not([in-voice-search-mode])) cr-composebox-voice-search {\n  display: none;\n}\n"])];
}
function getCss28() {
  return [getCss7(), getCss27(), i([`/* Copyright 2026 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=//resources/cr_elements/cr_icons_lit.css.js
 * #import=//resources/cr_components/composebox/composebox_shared_style.css.js
 * #scheme=relative
 * #include=cr-icons-lit composebox-shared-style
 * #css_wrapper_metadata_end */

:host {
  /* Embedders should define --cr-searchbox-min-width. */
  --cr-searchbox-width: var(--cr-searchbox-min-width);
  --cr-searchbox-border-radius: calc(0.5 * var(--cr-searchbox-height));
  --cr-searchbox-icon-width: 26px;
  --cr-searchbox-inner-icon-margin: 8px;
  --cr-searchbox-voice-icon-offset: 16px;
  --cr-searchbox-voice-search-button-width: 0px;
  --cr-compose-button-width: 104px;
  --cr-searchbox-icon-spacing: 11px;
  border-radius: var(--cr-searchbox-border-radius);
  font-size: var(--cr-searchbox-font-size, 16px);
  height: var(--cr-searchbox-height);
  width: var(--cr-searchbox-width);
  z-index: 99;
}

:host([searchbox-chrome-refresh-theming][dropdown-is-visible]) {
  --cr-searchbox-shadow: 0 0 12px 4px var(--color-searchbox-shadow);
}

:host([searchbox-chrome-refresh-theming]:not([searchbox-steady-state-shadow]):not([dropdown-is-visible])) {
  --cr-searchbox-shadow: none;
}

:host-context([searchbox-width-behavior_='revert']):host([can-show-secondary-side]:not([dropdown-is-visible])) {
  --cr-searchbox-width: var(--cr-searchbox-min-width);
}

/**
 * Show the secondary side if it can be shown and is currently available to be
 * shown.
 */
:host([can-show-secondary-side][has-secondary-side]) {
  --cr-searchbox-secondary-side-display: block;
}

:host([is-dark]) {
  --cr-searchbox-shadow: 0 2px 6px 0 var(--color-searchbox-shadow);
}

:host-context([energy-effect-enabled_][energy-effect-variant_='energy-effect-original']) {
  --cr-searchbox-shadow: 0 4px 18px -2px rgba(60, 64, 67, 0.12);
}

:host-context([energy-effect-enabled_][energy-effect-variant_='energy-effect-darker-shadow']) {
  --cr-searchbox-shadow: 0 4px 18px -2px rgba(60, 64, 67, 0.20);
}

:host-context([energy-effect-enabled_][energy-effect-variant_='energy-effect-fusebox']) {
  --cr-searchbox-shadow: 0 2px 6px 0 rgba(60, 64, 67, 0.16);
}

:host([searchbox-voice-search-enabled_]) {
  --cr-searchbox-voice-search-button-width: var(--cr-searchbox-icon-width);
}

:host([searchbox-lens-search-enabled_]) {
  --cr-searchbox-voice-icon-offset: 53px;
}

@media (forced-colors: active) {
  :host {
    border: 1px solid ActiveBorder;
  }
}

:host([dropdown-is-visible]) {
  box-shadow: none;
}

#inputWrapper {
  background-color: var(--color-searchbox-background);
  border-radius: var(--cr-searchbox-border-radius);
  box-shadow: var(--cr-searchbox-shadow);
  display: flex;
  flex-direction: column;
  height: auto;
  min-height: var(--cr-searchbox-height);
  position: relative;
  width: 100%;
}

/* Renders the border as an inset overlay to prevent expanding the outer
 * height or shifting inner coordinates, maintaining layout alignment
 * with Composebox. */
#inputWrapper::after {
  border: var(--cr-searchbox-border, none);
  border-radius: inherit;
  box-sizing: border-box;
  content: '';
  inset: 0;
  pointer-events: none;
  position: absolute;
}

:host([in-voice-search-mode]) #inputWrapper {
  display: none;
}

:host([ntp-realbox-next-enabled]) #inputWrapper {
  /* Elements like the IPH must overflow. */
  overflow: visible;
}

:host([ntp-realbox-next-enabled]) .dropdownContainer {
  border-radius: 0 0 var(--cr-searchbox-border-radius) var(--cr-searchbox-border-radius);
  overflow: hidden;
}

:host([ntp-realbox-next-enabled]) .contextualEntrypointContainer {
  overflow: visible;
  left: unset;
  position: relative;
  right: unset;
  top: unset;
  width: 100%;
}

cr-searchbox-input::part(searchbox-input) {
  align-self: center;
  padding-top: 2px;
}

:host([compose-button-enabled]) cr-searchbox-input::part(searchbox-input) {
  padding-inline-end: 16px;
}

:host([ntp-realbox-next-enabled]) cr-searchbox-input::part(searchbox-input) {
  padding-inline-start: 0;
}

:host([multi-line-enabled][ntp-realbox-next-enabled]) cr-searchbox-input::part(searchbox-input) {
  padding-bottom: 10px;
  padding-top: 16px;
}

:host([searchbox-chrome-refresh-theming]) cr-searchbox-input::part(searchbox-input)::selection {
  background-color: var(--color-searchbox-selection-background);
  color: var(--color-searchbox-selection-foreground);
}

:host(:not([ntp-realbox-next-enabled])) cr-searchbox-input::part(icon) {
  align-self: center;
}

.searchbox-icon-button {
  background-color: transparent;
  background-position: center;
  background-repeat: no-repeat;
  background-size: 21px 21px;
  border: none;
  border-radius: 2px;
  cursor: pointer;
  height: 100%;
  outline: none;
  padding: 0;
  pointer-events: auto;
  position: static;
  right: 16px;
  width: var(--cr-searchbox-icon-width);
  z-index: 100;
}

/* When voice/lens icons are monochrome, they are webkit mask images.
 * Webkit mask images hide borders so container rules are created to
 * show focus borders on these icons. */
.searchbox-icon-button-container {
  align-items: center;
  border-radius: 50%;
  display: flex;
  flex-shrink: 0;
  height: 36px;
  justify-content: center;
  position: relative;
  width: 36px;
  z-index: 100;
}

:host(:not([ntp-realbox-next-enabled])) .searchbox-icon-button-container {
  align-self: center;
}

@media (forced-colors: active) {
  .searchbox-icon-button-container {
    background-color: ButtonText;
  }
  .searchbox-icon-button-container:focus-within {
    outline: 2px solid Highlight;
    outline-offset: 2px;
  }
}

:host-context(.focus-outline-visible) .searchbox-icon-button-container:focus-within {
  box-shadow: 0 0 0 2px var(--cr-focus-outline-color);
}

:host(:not([use-webkit-search-icons_])) #voiceSearchButton {
  background-image: url("/newtab/chromium/cr_components/searchbox/icons/mic_old.svg");
}

:host(:not([use-webkit-search-icons_])) #lensSearchButton {
  background-image: url("/newtab/chromium/cr_components/searchbox/icons/camera.svg");
}

:host([use-webkit-search-icons_]) #voiceSearchButton {
  -webkit-mask-image: url("/newtab/chromium/cr_components/searchbox/icons/mic_old.svg");
}

:host([use-webkit-search-icons_]) #lensSearchButton {
  -webkit-mask-image: url("/newtab/chromium/cr_components/searchbox/icons/camera.svg");
}

:host([use-webkit-search-icons_]) #voiceSearchButton,
:host([use-webkit-search-icons_]) #lensSearchButton {
  -webkit-mask-position: center;
  -webkit-mask-repeat: no-repeat;
  -webkit-mask-size: 21px 21px;
  background-color: var(--color-searchbox-lens-voice-icon-background);
}

:host([use-webkit-search-icons_][compose-button-enabled]) #voiceSearchButton,
:host([use-webkit-search-icons_][compose-button-enabled]) #lensSearchButton {
  background-color: #1F1F1F;
}

:host([compose-button-enabled][searchbox-lens-search-enabled_]) {
  --cr-searchbox-voice-icon-offset: calc(16px + 2 * var(--cr-searchbox-icon-spacing) + var(--cr-searchbox-icon-width) + var(--cr-compose-button-width));
}

:host([ntp-realbox-next-enabled]) cr-searchbox-input::part(searchbox-input)::placeholder {
  color: var(--cr-composebox-input-placeholder-color, var(--color-composebox-type-ahead));
  font-weight: 400;
}

:host([ntp-realbox-next-enabled]) cr-searchbox-input::part(searchbox-input):focus::placeholder {
  visibility: visible;
}

:host([ntp-realbox-next-enabled]) {
  --cr-searchbox-border-radius: 28px;
  --cr-searchbox-dropdown-padding-bottom: 10px;
  --cr-searchbox-dropdown-padding-top: 10px;
  --cr-searchbox-height: 56px;
  /* Cannot touch this searchbox icon size var;
    it's used by the rainbow outline AI button class in
    searchbox_compose_button.css. Should be different
    from other searchbox icons (composebox, voice, lens). */
  --cr-searchbox-icon-size: 40px;
  --cr-searchbox-voice-lens-size: 36px;
  --contextual-entrypoint-and-carousel-context-menu-container-padding-inline-start: 0px;
  --text-input-inline-start-spacing: 16px;

  /* Used in search-animated-glow-element. */
  --search-animated-glow-drag-drop-placeholder-top: 16px;
}

:host-context([energy-effect-enabled_]):host([ntp-realbox-next-enabled]) {
  --cr-searchbox-border: 1px solid #DADCE0;
}

:host-context([energy-effect-enabled_][energy-effect-variant_='pre-energy-effect-with-border']):host([ntp-realbox-next-enabled]) {
  --cr-searchbox-border: 1px solid #CCCFD9;
}

cr-searchbox-dropdown::part(dropdown-content) {
  background-color: unset;
  border-radius: unset;
  box-shadow: unset;
  gap: unset;
  margin-bottom: unset;
  overflow: unset;
  padding-top: unset;
}

:host([ntp-realbox-next-enabled]) cr-composebox-contextual-entrypoint-and-menu::part(context-menu-entrypoint-icon) {
  --cr-icon-button-icon-size: 24px;
  --cr-icon-button-margin-start: 2px;
  --cr-icon-button-size: 36px;
}

:host([ntp-realbox-next-enabled]:not([context-menu-glif-animation-state='ineligible'])) cr-composebox-contextual-entrypoint-and-menu::part(context-menu-entrypoint-icon) {
  /* Account for the margin of the glow container */
  --cr-icon-button-margin-start: 0px;
}

:host([ntp-realbox-next-enabled]) .context-menu-container {
  padding-block: 10px;
}

:host([ntp-realbox-next-enabled]) cr-composebox-contextual-entrypoint-and-menu {
  color: var(--color-searchbox-results-foreground);
  padding-inline-end: 6px;
  padding-inline-start: 8px;
  position: relative;
  z-index: 100;
}

:host-context([ntp-realbox-next-enabled][context-menu-glif-animation-state='ineligible']) cr-composebox-contextual-entrypoint-and-menu {
  padding-inline-end: 14px;
}

#inputContainer {
  background-color: var(--color-searchbox-background);
  border-radius: 26px;
  box-shadow: var(--cr-searchbox-shadow);
  display: flex;
  flex-direction: column;
  width: 100%;
}

:host([ntp-realbox-next-enabled][dropdown-is-visible]),
:host([ntp-realbox-next-enabled]:not([context-files-count_='0'])) {
  box-shadow: none;
}

.searchbox-icon-button-container.lens {
  margin-inline-end: 10px;
}

.searchbox-icon-button-container.voice {
  margin-inline-end: 4px;
}

:host([ntp-realbox-next-enabled]) .searchbox-icon-button-container.lens:hover,
:host([ntp-realbox-next-enabled]) .searchbox-icon-button-container.voice:hover {
  background-color: var(--color-new-tab-page-realbox-next-icon-hover);
  border-radius: 50%;
}

:host([ntp-realbox-next-enabled]) cr-searchbox-input::part(icon) {
  display: none;
}

:host([ntp-realbox-next-enabled]) .searchbox-icon-button-container {
  top: 10px;
}

:host([ntp-realbox-next-enabled][compose-button-enabled]
    [searchbox-lens-search-enabled_]) .searchbox-icon-button-container.lens {
  inset-inline-end: calc(var(--cr-searchbox-icon-spacing) + var(--cr-compose-button-width));
}

:host([ntp-realbox-next-enabled][compose-button-enabled]
    [searchbox-lens-search-enabled_]) .searchbox-icon-button-container.voice {
  inset-inline-end: calc(var(--cr-searchbox-voice-lens-size)
      + var(--cr-searchbox-icon-spacing) + var(--cr-compose-button-width));
}

:host([ntp-realbox-next-enabled][is-dragging-file]) {
  --cr-searchbox-compose-button-z-index: 99;
}

:host([ntp-realbox-next-enabled][is-dragging-file]) cr-searchbox-input::part(searchbox-input),
:host([ntp-realbox-next-enabled][is-dragging-file]) cr-searchbox-input::part(icon),
:host([ntp-realbox-next-enabled][is-dragging-file]) .searchbox-icon-button-container,
:host([ntp-realbox-next-enabled][is-dragging-file]) .searchbox-icon-button {
  opacity: 0;
}

:host([ntp-realbox-next-enabled][dropdown-is-visible]) .dropdownContainer {
  padding-bottom: 10px;
}

:host([ntp-realbox-next-enabled])
    .contextualEntrypointContainerCompact {
  box-shadow: none;
  width: auto;
}

:host-context([webui-rounded-icons]):host(:not([use-webkit-search-icons_])) #voiceSearchButton {
  background-image: url("/newtab/chromium/cr_components/searchbox/icons/mic.svg");
}

:host-context([webui-rounded-icons]):host([use-webkit-search-icons_]) #voiceSearchButton {
  -webkit-mask-image: url("/newtab/chromium/cr_components/searchbox/icons/mic.svg");
}
`])];
}
var icons = "/newtab/chromium/cr_components/searchbox/icons/";
var engine = "google";
var engineName = "Google";
var request = null;
var current = null;
var sequence = 0;
var previousHistory = [];
var previousSearches = [];
var deleted = /* @__PURE__ */ new Set();
var engineUrls = { google: "https://www.google.com/search?q=", bing: "https://www.bing.com/search?q=", duckduckgo: "https://duckduckgo.com/?q=", yahoo: "https://search.yahoo.com/search?p=", youtube: "https://www.youtube.com/results?search_query=", wikipedia: "https://wikipedia.org/w/index.php?search=", netflix: "https://www.netflix.com/search?q=", googlemaps: "https://www.google.com/maps/search/", ebay: "https://www.ebay.com/sch/?_nkw=", amazon: "https://www.amazon.com/s?k=", amazom: "https://www.amazon.com/s?k=", ecosia: "https://www.ecosia.org/search?q=" };
function searchUrl(text) {
  return (engineUrls[engine] || engineUrls.google) + encodeURIComponent(text);
}
function navigationUrl(text) {
  try {
    if (/\s/.test(text)) return null;
    const u5 = new URL(text.includes("://") ? text : `https://${text}`);
    return ["https:", "http:"].includes(u5.protocol) && !u5.username && !u5.password && (text.includes("://") || u5.hostname.includes(".") || u5.hostname === "localhost" || u5.hostname.startsWith("[")) ? u5.href : null;
  } catch {
    return null;
  }
}
function classify(text, query, search) {
  const i7 = text.toLocaleLowerCase().indexOf(query.toLocaleLowerCase());
  if (!query || i7 < 0) return [{ offset: 0, style: query && search ? 2 : 0 }];
  const styles = [];
  if (i7) styles.push({ offset: 0, style: search ? 2 : 0 });
  styles.push({ offset: i7, style: search ? 0 : 2 });
  if (i7 + query.length < text.length) styles.push({ offset: i7 + query.length, style: search ? 2 : 0 });
  return styles;
}
function historicalSearch(url) {
  try {
    const u5 = new URL(url);
    if (/(^|\.)google\.[a-z.]+$/.test(u5.hostname) && u5.pathname === "/search") return u5.searchParams.get("q");
    if (u5.hostname === "www.bing.com" && u5.pathname === "/search") return u5.searchParams.get("q");
    if (u5.hostname === "duckduckgo.com") return u5.searchParams.get("q");
    if (u5.hostname === "search.yahoo.com" && u5.pathname === "/search") return u5.searchParams.get("p");
    if (u5.hostname === "www.youtube.com" && u5.pathname === "/results") return u5.searchParams.get("search_query");
  } catch {
  }
  return null;
}
function historyMatches(query) {
  return previousHistory.filter((h4) => !deleted.has(h4.url) && (!query || `${h4.text} ${h4.url}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()))).slice(0, query ? 3 : 8).map((h4) => {
    const term = historicalSearch(h4.url), text = term || h4.text;
    return createAutocompleteMatch({
      contents: text,
      contentsClass: classify(text, query, !!term),
      description: term ? `${engineName} Search` : h4.url.replace(/^https?:\/\//, ""),
      descriptionClass: [{ offset: 0, style: term ? 4 : 1 }],
      a11yLabel: `${text}, ${h4.url}`,
      fillIntoEdit: term || h4.url,
      destinationUrl: h4.url,
      isSearchType: !!term,
      type: term ? "search-history" : "history-title",
      iconPath: icons + (term ? "history_cr23.svg" : "page_cr23.svg"),
      supportsDeletion: true,
      removeButtonA11yLabel: `Remove ${text} from history`,
      historyUrl: h4.url
    });
  });
}
function publish2() {
  if (!request) return;
  const query = request.input.trim(), matches = [];
  if (query) {
    const url = navigationUrl(query);
    matches.push(createAutocompleteMatch({
      contents: query,
      fillIntoEdit: query,
      destinationUrl: url || searchUrl(query),
      a11yLabel: `${query}${url ? "" : `, ${engineName} Search`}`,
      isSearchType: !url,
      allowedToBeDefaultMatch: true,
      type: url ? "url-what-you-typed" : "search-what-you-typed",
      iconPath: icons + (url ? "page_cr23.svg" : "search_cr23.svg"),
      description: url ? "" : `${engineName} Search`,
      descriptionClass: [{ offset: 0, style: 4 }]
    }));
  }
  matches.push(...historyMatches(query));
  const seen = new Set(matches.map((m3) => m3.contents.toLocaleLowerCase()));
  if (query) for (const text of previousSearches) {
    if (seen.has(text.toLocaleLowerCase())) continue;
    seen.add(text.toLocaleLowerCase());
    matches.push(createAutocompleteMatch({
      contents: text,
      contentsClass: classify(text, query, true),
      a11yLabel: `${text}, search`,
      fillIntoEdit: text,
      destinationUrl: searchUrl(text),
      isSearchType: true,
      type: "search-suggest",
      iconPath: icons + "search_cr23.svg"
    }));
  }
  current = { input: request.input, matches: matches.slice(0, 8), suggestionGroupsMap: {}, queryId: request.queryId, sequenceId: ++sequence };
  callbackRouter.autocompleteResultChanged.emit(current);
}
function renderedMatch(index, url) {
  const box = document.querySelector("ntp-app")?.shadowRoot?.querySelector("ntp-searchbox");
  const match = box?.result?.matches[index];
  return match && (!url || match.destinationUrl === url) ? match : null;
}
Object.assign(handler, {
  queryAutocomplete(queryId, tabId, input, preventInlineAutocomplete) {
    request = { queryId, input, preventInlineAutocomplete, id: 0 };
    publish2();
    request.id = send("suggest", { query: input.trim() });
  },
  stopAutocomplete(clearResult) {
    request = null;
    send("stop-suggest");
    if (clearResult) {
      current = null;
      previousSearches = [];
    }
  },
  openAutocompleteMatch(index, url, showing, button, modifiers) {
    const match = renderedMatch(index, url);
    if (!match) return;
    if (button === 1 || modifiers?.ctrlKey || modifiers?.metaKey || modifiers?.shiftKey)
      window.open(match.destinationUrl, "_blank");
    else send("navigate", { text: match.historyUrl || match.fillIntoEdit, forceSearch: match.type === "search-suggest" });
  },
  deleteAutocompleteMatch(index, url) {
    const match = renderedMatch(index, url);
    if (!match?.supportsDeletion) return;
    deleted.add(match.historyUrl);
    send("delete-history", { url: match.historyUrl });
    if (!request && current) request = { queryId: current.queryId, input: current.input, id: 0 };
    publish2();
  },
  onFocusChanged() {
  },
  onNavigationLikely() {
  },
  onPopupSelectionChanged() {
  }
});
hostEvents.addEventListener("state", ({ detail }) => {
  engine = detail.engine || "google";
  engineName = { google: "Google", bing: "Bing", duckduckgo: "DuckDuckGo", yahoo: "Yahoo", youtube: "YouTube" }[engine] || engine;
});
hostEvents.addEventListener("suggestions", ({ detail }) => {
  if (!request || detail.id !== request.id || detail.query !== request.input.trim()) return;
  previousHistory = detail.history || [];
  if (detail.complete) previousSearches = detail.searches || [];
  publish2();
});
hostEvents.addEventListener("history-deleted", ({ detail }) => {
  if (detail.success) {
    previousHistory = previousHistory.filter((h4) => h4.url !== detail.url);
  } else deleted.delete(detail.url);
  if (request) request.id = send("suggest", { query: request.input.trim() });
});
var QuartzSearchbox = class extends SearchboxMixin(CrLitElement) {
  static get properties() {
    return { placeholder: { type: String }, engine: { type: String } };
  }
  static get styles() {
    return getCss28();
  }
  #placeholder = "Search Google or type a URL";
  get placeholder() {
    return this.#placeholder;
  }
  set placeholder(_2) {
    this.#placeholder = _2;
  }
  #engine = "Google";
  get engine() {
    return this.#engine;
  }
  set engine(_2) {
    this.#engine = _2;
  }
  listenerId;
  refreshIcons = () => {
    this.$.input?.shadowRoot?.querySelector("cr-searchbox-icon")?.requestUpdate("match");
    for (const match of this.$.matches?.shadowRoot?.querySelectorAll("cr-searchbox-match") || [])
      match.shadowRoot?.querySelector("cr-searchbox-icon")?.requestUpdate("match");
  };
  constructor() {
    super();
    this.searchboxIcon = "/newtab/chromium/cr_components/searchbox/icons/search_cr23.svg";
    this.searchboxAriaDescription = "Search or type a URL";
  }
  connectedCallback() {
    super.connectedCallback();
    this.listenerId = callbackRouter.autocompleteResultChanged.addListener(this.onAutocompleteResultChanged.bind(this));
    hostEvents.addEventListener("icons-updated", this.refreshIcons);
  }
  disconnectedCallback() {
    callbackRouter.removeListener(this.listenerId);
    hostEvents.removeEventListener("icons-updated", this.refreshIcons);
    super.disconnectedCallback();
  }
  pageHandler() {
    return handler;
  }
  getInputElement() {
    return this.$.input;
  }
  getDropdownElement() {
    return this.$.matches;
  }
  getWrapperElement() {
    return this.$.inputWrapper;
  }
  render() {
    return b2`
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
    </div>`;
  }
};
customElements.define("ntp-searchbox", QuartzSearchbox);
function getCss29() {
  return [getCss10(), getCss7(), i([`/* Copyright 2024 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=chrome://resources/cr_elements/cr_shared_style_lit.css.js
 * #import=chrome://resources/cr_elements/cr_icons_lit.css.js
 * #scheme=relative
 * #include=cr-shared-style-lit cr-icons-lit
 * #css_wrapper_metadata_end */

:host {
  --cr-composebox-input-placeholder-color: var(
    --color-new-tab-page-common-input-placeholder
  );
  --cr-focus-outline-color: var(--color-new-tab-page-focus-ring);
  --cr-searchbox-height: 48px;
  --cr-searchbox-shadow: 0 1px 6px 0 var(--color-searchbox-shadow);
  --cr-searchbox-icon-left-position: 12px;
  --cr-searchbox-icon-size-in-searchbox: 20px;
  --cr-searchbox-icon-top-position: 0;
  --cr-searchbox-min-width: var(--ntp-search-box-width);
  --cr-searchbox-results-search-icon-size: 20px;
  --ntp-theme-text-shadow: none;
  --ntp-one-google-bar-height: 56px;
  --ntp-search-box-width: 337px;
  --ntp-menu-shadow:
    var(--color-new-tab-page-menu-inner-shadow) 0 1px 2px 0,
    var(--color-new-tab-page-menu-outer-shadow) 0 2px 6px 2px;
  --ntp-module-width: 360px;
  --ntp-module-layout-width: 360px;
  --ntp-module-border-radius: 16px;
  --ntp-module-item-border-radius: 12px;
  --ntp-protected-icon-background-color: transparent;
  --ntp-protected-icon-background-color-hovered: rgba(255, 255, 255, 0.1);
  --ntp-scrim-opacity_: 1;
  --threads-rail-width: 76px;
  --threads-rail-focus-outline-color: var(--color-new-tab-page-focus-ring);


}

/**
 * Hide everything that should not display when the composebox is open
 * so they are not tabbable.
 */
:host([show-composebox_]:not([ntp-realbox-next-enabled_])) cr-most-visited,
:host([show-composebox_]:not([ntp-realbox-next-enabled_])) ntp-modules,
:host([show-composebox_]:not([ntp-realbox-next-enabled_]))
  #backgroundImageAttribution,
:host([show-composebox_]:not([ntp-realbox-next-enabled_]))
  ntp-customize-buttons,
:host([show-composebox_]:not([ntp-realbox-next-enabled_]))
  setup-list-module-wrapper {
  display: none;
}

:host([ntp-realbox-next-enabled_]) {
  --cr-searchbox-height: 56px;
  --ntp-scrim-opacity_: 0.5;
}

/**
 * Maintain a larger width if the secondary side can be shown and was at any
 * point available to be shown.
 */
:host([realbox-can-show-secondary-side][realbox-had-secondary-side]),
:host([realbox-can-show-secondary-side]) {
  --ntp-search-box-width: 746px;
}

@media (min-width: 560px) {
  :host {
    --ntp-search-box-width: 449px;
  }
}

@media (min-width: 672px) {
  :host {
    --ntp-search-box-width: 561px;
  }
}

/*A module width of 768px with 18px gaps on each side. */
@media (min-width: 804px) {
  :host {
    --ntp-module-layout-width: 768px;
    --ntp-module-width: 768px;
  }
}

cr-most-visited {
  --add-shortcut-background-color: var(
    --color-new-tab-page-add-shortcut-background
  );
  --add-shortcut-foreground-color: var(
    --color-new-tab-page-add-shortcut-foreground
  );
  --tile-hover-color: var(
    --color-new-tab-page-add-shortcut-background-hovered
  );
  --cr-menu-shadow: var(--ntp-menu-shadow);
  --most-visited-focus-shadow: var(--ntp-focus-shadow);
  --most-visited-text-color: var(--color-new-tab-page-most-visited-foreground);
  --most-visited-text-shadow: var(--ntp-theme-text-shadow);
}

:host([show-background-image_]) {
  --ntp-theme-text-shadow:
    0.5px 0.5px 1px rgba(0, 0, 0, 0.5), 0px 0px 2px rgba(0, 0, 0, 0.2),
    0px 0px 10px rgba(0, 0, 0, 0.1);
  --ntp-protected-icon-background-color: rgba(0, 0, 0, 0.6);
  --ntp-protected-icon-background-color-hovered: rgba(0, 0, 0, 0.7);
}

/* The styles to create a stacking context for the OGB-related elements.
 * The OGB has z-index of 1000 (set by inline styling),
 * and it needs to be segregated to get the OGB scrimmed. */
#oneGoogleBarStackingContext {
  display: flex;
  border: 0;
  top: 0;
  width: 100%;
  /* OGB is above any other element by default to keep the original behavior. */
  z-index: 1000;
}

/* When scrim is applied, OGB must be behind it. */
:host([show-scrim_]) #oneGoogleBarStackingContext {
  z-index: 0;
}

#oneGoogleBarScrim {
  background: linear-gradient(
    rgba(0, 0, 0, 0.25) 0%,
    rgba(0, 0, 0, 0.12) 45%,
    rgba(0, 0, 0, 0.05) 65%,
    transparent 100%
  );
  height: 80px;
  position: absolute;
  top: 0;
  width: 100%;
}

#oneGoogleBarScrim[fixed] {
  /* Prevent scrim from bouncing when overscrolling. */
  position: fixed;
}

#oneGoogleBar {
  height: 100%;
  position: absolute;
  top: 0;
  width: 100%;
}

#content {
  align-items: center;
  display: flex;
  flex-direction: column;
  height: calc(100vh - var(--ntp-one-google-bar-height));
  min-width: fit-content; /* Prevents OneGoogleBar cutoff at 500% zoom. */
  padding-top: var(--ntp-one-google-bar-height);
  position: relative;
  z-index: 1;
}

/*
 * Below 900px, the threads rail will shift the main content aside.
 * This happens in conjunction with the realbox width change as well,
 * when the total width reaches 824px + 76px (realbox + rail).
 */
@media (max-width: 900px) {
  #content[ntp-threads-rail-open] {
    margin-inline-start: var(--threads-rail-width);
    width: calc(100% - var(--threads-rail-width));
  }
}

:host([show-composebox_]) #content,
:host([show-voice-search-scrim_]) #content {
  z-index: unset;
}

#logo {
  margin-bottom: var(--ntp-logo-margin-bottom, 38px);
}

:host([show-composebox_]) #logo,
:host([show-voice-search-scrim_]) #logo,
:host([ntp-realbox-next-enabled_][show-scrim_]) #logo {
  z-index: 2;
}

#searchboxContainer {
  --cr-focus-outline-color: var(--color-searchbox-results-icon-focused-outline);
  display: inherit;
  margin-bottom: 16px;
  position: relative;
  z-index: 2;
}

#modules:not([hidden]) {
  /* We use animation instead of transition to allow a fade-in out of
     display: none. */
  animation: 300ms ease-in-out fade-in-animation;
}

@keyframes fade-in-animation {
  0% {
    opacity: 0;
  }
  100% {
    opacity: 1;
  }
}

ntp-searchbox {
  visibility: hidden;
}

ntp-searchbox[shown] {
  visibility: visible;
}

#themeAttribution {
  align-self: flex-start;
  bottom: 16px;
  color: var(--color-new-tab-page-secondary-foreground);
  margin-inline-start: 16px;
  position: fixed;
}

#backgroundImageAttribution {
  border-radius: 8px;
  bottom: 16px;
  color: var(--color-new-tab-page-attribution-foreground);
  line-height: 20px;
  max-width: 50vw;
  padding: 8px;
  position: fixed;
  z-index: -1;
  background-color: var(--ntp-protected-icon-background-color);
  text-shadow: none;
}

#backgroundImageAttribution:hover {
  background-color: var(--ntp-protected-icon-background-color-hovered);
}

:host-context([dir='ltr']) #backgroundImageAttribution {
  left: 16px;
}

:host-context([dir='rtl']) #backgroundImageAttribution {
  right: 16px;
}

#backgroundImageAttribution1Container {
  align-items: center;
  display: flex;
  flex-direction: row;
}

#linkIcon {
  -webkit-mask-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/link_old.svg");
  -webkit-mask-repeat: no-repeat;
  -webkit-mask-size: 100%;
  background-color: var(--color-new-tab-page-attribution-foreground);
  height: 16px;
  margin-inline-end: 8px;
  width: 16px;
}

#backgroundImageAttribution1,
#backgroundImageAttribution2 {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

#backgroundImageAttribution1 {
  font-size: 0.875rem;
}

#backgroundImageAttribution2 {
  font-size: 0.75rem;
}

#customizeButtons {
  bottom: 16px;
  position: fixed;
}

:host-context([dir='ltr']) #customizeButtons {
  right: 16px;
}

:host-context([dir='rtl']) #customizeButtons {
  left: 16px;
}

#contentBottomSpacer {
  flex-shrink: 0;
  height: 32px;
  width: 1px;
}

svg {
  position: fixed;
}

#composebox {
  --color-composebox-submit-button-background: var(
    --color-new-tab-page-composebox-submit-button-background);

  --cr-composebox-background-color: var(--color-composebox-background);
  --cr-composebox-expanded-border-radius: 28px;
  --cr-composebox-submit-border-radius: 100px;
  --cr-composebox-submit-height: 36px;
  --cr-composebox-submit-icon-size: 24px;
  --cr-composebox-submit-icon-offset: 0;
  --cr-composebox-submit-width: 48px;
  --cr-composebox-width: var(--ntp-search-box-width);

  --text-input-inline-end-spacing: 90px;
}

#composebox::part(cancel-icon) {
  inset-inline-end: 16px;
}

/* Adjust voice-icon to maintain 12px gap from cancel-icon (which is 24px wide
   with inset-inline-end of 16px). 16px + 24px + 12px = 52px). */
#composebox::part(voice-icon) {
  inset-inline-end: 52px;
}

#composebox,
ntp-lens-upload-dialog {
  left: 0;
  position: absolute;
  right: 0;
  top: 0;
  z-index: 101;
}

#scrim {
  background: var(--color-composebox-scrim-background);
  inset: 0;
  opacity: var(--ntp-scrim-opacity_);
  position: fixed;
  transition: all 250ms ease;
  z-index: 1;
}

#webstoreToast {
  padding: 16px;
}

/*
  Microsoft Auth's iframe is used for script sandboxing only and requires no
  user interaction. It does not need to be visible.
*/
#microsoftAuth {
  display: none;
}

#dialogAnchor {
  anchor-name: --dialog-anchor;
  inset-inline-start: 0;
  position: absolute;
  top: 0;
}

#composeboxDialog {
  background: transparent;
  border: none;
  inset-inline-start: anchor(start);
  margin: 0;
  overflow: visible;
  position: absolute;
  position-anchor: --dialog-anchor;
  top: anchor(top);
  z-index: 100;
}

#undoToast {
  flex-grow: 1;
}

@container style(--cr-animations-disabled: 1) {
  #modules:not([hidden]) {
    animation: none;
  }
  #scrim {
    transition: none;
  }
}

#voiceSearchDialog {
  --color-composebox-submit-button-background: var(
      --color-new-tab-page-composebox-submit-button-background);
  --color-composebox-submit-button-icon: white;
  --cr-composebox-submit-border-radius: 100px;
  --cr-composebox-submit-height: 36px;
  --cr-composebox-submit-icon-offset: 0;
  --cr-composebox-submit-icon-size: 24px;
  --cr-composebox-submit-width: 48px;
  background: var(--color-composebox-background);
  border: none;
  border-radius: 28px;
  bottom: auto;
  box-shadow: var(--cr-searchbox-shadow);
  font-family: inherit;
  height: 128px;
  inset-inline-start: anchor(start);
  margin: 0;
  overflow: hidden;
  padding: 0;
  position: absolute;
  position-anchor: --dialog-anchor;
  top: anchor(top);
  width: var(--ntp-search-box-width);
}

#voiceSearchDialog:has(#voiceSearch[live-transcript-enabled]) {
  height: auto;
  max-height: 254px;
  min-height: 128px;
}

#voiceSearchDialog:has(#voiceSearch[audio-wave-enabled]) {
  height: 168px;
  min-height: 168px;
}

/* Disable default scrim and use custom scrim instead. */
#voiceSearchDialog::backdrop {
  display: none;
}

#voiceSearchCardContainer {
  height: 100%;
  position: relative;
  width: 100%;
}

#voiceSearchDialog:has(#voiceSearch[live-transcript-enabled])
    #voiceSearchCardContainer {
  height: auto;
}

#voiceSearchDialog:has(#voiceSearch[audio-wave-enabled])
    #voiceSearchCardContainer {
  height: 100%;
}

#voiceSearch {
  height: 100%;
  width: 100%;
  --composebox-voice-search-top-padding: 9px;
  --composebox-cancel-button-top: 10px;
  --voice-bottom-actions-bottom: 12px;
  --voice-bottom-actions-inset-inline-end: 12px;
  /* Minimum height used for error modes, etc., where 'composebox_voice_search'
   * determines the composebox height. In normal voice search mode, 'search-animated-glow'
   * determines the composebox height and min-height is ignored.
   */
  --voice-search-minimum-height: 128px;
}

#voiceSearch[live-transcript-enabled] {
  height: auto;
  inset: auto;
  position: static;
}

#voiceSearch[audio-wave-enabled] {
  --voice-bottom-actions-inset-inline-end: 16px;
  --voice-search-minimum-height: 168px;
  height: 100%;
  inset: 0;
  position: absolute;
}

#voiceSearch::part(voice-stop-button) {
  position: relative;
}

#voiceSearch::part(submit) {
  position: relative;
}

#voiceSearchGlow {
  inset: 0;
  pointer-events: none;
  position: absolute;
}

#voiceSearchGlow::part(full-container-overlay) {
  min-block-size: 0;
  position: absolute;
}

#voiceSearchDialog:has(#voiceSearch[audio-wave-enabled])
    #voiceSearchGlow {
  --recording-wave-top-padding: 4px;
}

#voiceSearchDialog:has(#voiceSearch[audio-wave-enabled])
    #voiceSearchGlow::part(full-container-overlay) {
  height: 48px;
  inset-inline: 0;
  padding: 0;
  top: 49px;
}

:host-context([webui-rounded-icons]) {
  #linkIcon {
    -webkit-mask-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/link.svg");
  }
}
`])];
}
function getCss30() {
  return [getCss3(), i([`/* Copyright 2024 The Chromium Authors
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file. */

/* #css_wrapper_metadata_start
 * #type=style-lit
 * #import=chrome://resources/cr_elements/cr_hidden_style_lit.css.js
 * #scheme=relative
 * #include=cr-hidden-style-lit
 * #css_wrapper_metadata_end */

:host {
  --ntp-logo-height: 168px;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  justify-content: flex-end;
  min-height: var(--ntp-logo-height);
}

/*
 * In case we use the boxed container for the Doodle, we can borrow 20px from
 * the bottom margin to make the Doodle logo larger.
 */
:host([doodle-boxed_][show-tight-doodle-boxing_]) {
  --ntp-logo-margin-bottom: 18px;
}

:host([doodle-boxed_]) {
  justify-content: flex-end;
}

#logo {
  forced-color-adjust: none;
  height: 92px;
  width: 272px;
}

:host([use-google-logo26_]) #logo {
  height: 82px;
  width: 270px;
}

:host([single-colored]) #logo {
  -webkit-mask-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/google_logo.svg");
  -webkit-mask-repeat: no-repeat;
  -webkit-mask-size: 100%;
  background-color: var(--ntp-logo-color);
}

:host(:not([single-colored])) #logo {
  background-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/google_logo.svg");
  background-repeat: no-repeat;
  background-size: 100%;
}

:host([doodle-boxed_]:not([show-tight-doodle-boxing_])) #imageDoodle {
  background-color: var(--ntp-logo-box-color);
  border-radius: 20px;
  padding: 16px 24px;
}

:host([doodle-boxed_][show-tight-doodle-boxing_]) #imageDoodle {
  background-color: var(--ntp-logo-box-color);
  border-radius: 28px;
  padding: 16px;
}

#imageContainer {
  cursor: pointer;
  display: flex;
  height: fit-content;
  outline: none;
  position: relative;
  width: fit-content;
}

#imageContainer[tabindex='-1'] {
  cursor: auto;
}

:host-context(.focus-outline-visible) #imageContainer:focus {
  box-shadow: 0 0 0 2px rgba(var(--google-blue-600-rgb), .4);
}

#image {
  max-height: var(--ntp-logo-height);
  max-width: 100%;
}

:host([doodle-boxed_]) #image {
  max-height: 128px;
}

/*
 * The max-height of 156px + 32px of vertical padding + 18px of bottom margin
 * equals the height of the ntp-logo container + bottom margin, which is 206px.
 */
:host([doodle-boxed_][show-tight-doodle-boxing_]) #image {
  max-height: 156px;
}

#animation {
  height: 100%;
  pointer-events: none;
  position: absolute;
  width: 100%;
}

#doodle {
  position: relative;
}

#shareButton {
  background-color: var(--color-new-tab-page-doodle-share-button-background, none);
  border: none;
  height: 32px;
  min-width: 32px;
  padding: 0;
  position: absolute;
  width: 32px;
  bottom: 0;
}

:host-context([dir='ltr']) #shareButton {
  right: -40px;
}

:host-context([dir='rtl']) #shareButton {
  left: -40px;
}

#shareButtonIcon {
  width: 18px;
  height: 18px;
  margin: 7px;
  vertical-align: bottom;
  mask-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/share_unfilled_old.svg");
  background-color: var(--color-new-tab-page-doodle-share-button-icon, none);
}

:host-context([webui-rounded-icons]) #shareButtonIcon {
  mask-image: url("/newtab/chromium/chrome/browser/resources/new_tab_page/icons/share_unfilled.svg");
  mask-repeat: no-repeat;
  mask-size: contain;
}
`])];
}
var engines = { google: "Google", bing: "Bing", duckduckgo: "DuckDuckGo", yahoo: "Yahoo", youtube: "YouTube", wikipedia: "Wikipedia", netflix: "Netflix", googlemaps: "Google Maps", ebay: "eBay", amazon: "Amazon", amazom: "Amazon", ecosia: "Ecosia" };
var palettes = { light: { tile: 4293257192, dark: false }, dark: { tile: 4282729797, dark: true }, black: { tile: 4280558628, dark: true }, aqua: { tile: 4290900212, dark: false }, xmas: { tile: 4288872448, dark: true } };
var QuartzLogo = class extends CrLitElement {
  static get styles() {
    return [getCss30(), i`img {width:144px;height:144px;object-fit:contain;}`];
  }
  render() {
    return b2`<img src="quartz.png" alt="Quartz" width="144" height="144" draggable="false">`;
  }
};
customElements.define("quartz-logo", QuartzLogo);
var QuartzNewTab = class extends CrLitElement {
  static get styles() {
    return getCss29();
  }
  handleState = ({ detail }) => {
    this.theme(detail.theme);
    this.$.searchbox.engine = engines[detail.engine] || "Google";
    this.$.searchbox.placeholder = `Search ${this.$.searchbox.engine} or type a URL`;
  };
  refreshIcons = () => this.$.mostVisited.requestUpdate();
  async connectedCallback() {
    super.connectedCallback();
    await this.updateComplete;
    this.theme(document.documentElement.dataset.theme || "light");
    hostEvents.addEventListener("state", this.handleState);
    hostEvents.addEventListener("icons-updated", this.refreshIcons);
    send("state");
  }
  disconnectedCallback() {
    hostEvents.removeEventListener("state", this.handleState);
    hostEvents.removeEventListener("icons-updated", this.refreshIcons);
    super.disconnectedCallback();
  }
  theme(name) {
    const palette = palettes[name] || palettes.light;
    document.documentElement.dataset.theme = name in palettes ? name : "light";
    this.$.mostVisited.theme = { backgroundColor: { value: palette.tile }, isDark: palette.dark, useWhiteTileIcon: palette.dark };
    this.$.searchbox.toggleAttribute("is-dark", palette.dark);
  }
  render() {
    return b2`
    <div id="content">
      <quartz-logo id="logo"></quartz-logo>
      <div id="searchboxContainer"><ntp-searchbox id="searchbox" shown></ntp-searchbox></div>
      <cr-most-visited id="mostVisited" single-row reflow-on-overflow></cr-most-visited>
    </div>`;
  }
};
customElements.define("ntp-app", QuartzNewTab);
document.addEventListener("visibilitychange", () => {
  if (!document.hidden) send("state");
});
/*! Bundled license information:

@lit/reactive-element/css-tag.js:
  (**
   * @license
   * Copyright 2019 Google LLC
   * SPDX-License-Identifier: BSD-3-Clause
   *)

@lit/reactive-element/reactive-element.js:
lit-html/lit-html.js:
lit-element/lit-element.js:
lit-html/directive.js:
lit-html/async-directive.js:
lit-html/directives/repeat.js:
  (**
   * @license
   * Copyright 2017 Google LLC
   * SPDX-License-Identifier: BSD-3-Clause
   *)

lit-html/is-server.js:
  (**
   * @license
   * Copyright 2022 Google LLC
   * SPDX-License-Identifier: BSD-3-Clause
   *)

lit-html/directive-helpers.js:
  (**
   * @license
   * Copyright 2020 Google LLC
   * SPDX-License-Identifier: BSD-3-Clause
   *)
*/
