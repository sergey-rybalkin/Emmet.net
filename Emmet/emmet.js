var emmet = (() => {
  var __defProp = Object.defineProperty;
  var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
  var __getOwnPropNames = Object.getOwnPropertyNames;
  var __hasOwnProp = Object.prototype.hasOwnProperty;
  var __markAsModule = (target) => __defProp(target, "__esModule", { value: true });
  var __export = (target, all) => {
    for (var name in all)
      __defProp(target, name, { get: all[name], enumerable: true });
  };
  var __reExport = (target, module, copyDefault, desc) => {
    if (module && typeof module === "object" || typeof module === "function") {
      for (let key of __getOwnPropNames(module))
        if (!__hasOwnProp.call(target, key) && (copyDefault || key !== "default"))
          __defProp(target, key, { get: () => module[key], enumerable: !(desc = __getOwnPropDesc(module, key)) || desc.enumerable });
    }
    return target;
  };
  var __toCommonJS = /* @__PURE__ */ ((cache) => {
    return (module, temp) => {
      return cache && cache.get(module) || (temp = __reExport(__markAsModule({}), module, 1), cache && cache.set(module, temp), temp);
    };
  })(typeof WeakMap !== "undefined" ? /* @__PURE__ */ new WeakMap() : 0);

  // emmet.es.js
  var emmet_es_exports = {};
  __export(emmet_es_exports, {
    default: () => emmet_es_default,
    extract: () => extractAbbreviation,
    markup: () => markup,
    markupAbbreviation: () => abbreviation_es_default,
    parseMarkup: () => parse2,
    parseStylesheet: () => parse$1,
    parseStylesheetSnippets: () => convertSnippets,
    resolveConfig: () => resolveConfig,
    stringifyMarkup: () => stringify2,
    stringifyStylesheet: () => css,
    stylesheet: () => stylesheet,
    stylesheetAbbreviation: () => css_abbreviation_es_default
  });

  // ../node_modules/@emmetio/scanner/scanner.es.js
  function isNumber(code2) {
    return code2 > 47 && code2 < 58;
  }
  function isAlpha(code2, from, to) {
    from = from || 65;
    to = to || 90;
    code2 &= ~32;
    return code2 >= from && code2 <= to;
  }
  function isAlphaNumericWord(code2) {
    return isNumber(code2) || isAlphaWord(code2);
  }
  function isAlphaWord(code2) {
    return code2 === 95 || isAlpha(code2);
  }
  function isWhiteSpace(code2) {
    return code2 === 32 || code2 === 9 || code2 === 160;
  }
  function isSpace(code2) {
    return isWhiteSpace(code2) || code2 === 10 || code2 === 13;
  }
  function isQuote(code2) {
    return code2 === 39 || code2 === 34;
  }
  var Scanner = class {
    constructor(str, start, end) {
      if (end == null && typeof str === "string") {
        end = str.length;
      }
      this.string = str;
      this.pos = this.start = start || 0;
      this.end = end || 0;
    }
    eof() {
      return this.pos >= this.end;
    }
    limit(start, end) {
      return new Scanner(this.string, start, end);
    }
    peek() {
      return this.string.charCodeAt(this.pos);
    }
    next() {
      if (this.pos < this.string.length) {
        return this.string.charCodeAt(this.pos++);
      }
    }
    eat(match) {
      const ch = this.peek();
      const ok = typeof match === "function" ? match(ch) : ch === match;
      if (ok) {
        this.next();
      }
      return ok;
    }
    eatWhile(match) {
      const start = this.pos;
      while (!this.eof() && this.eat(match)) {
      }
      return this.pos !== start;
    }
    backUp(n) {
      this.pos -= n || 1;
    }
    current() {
      return this.substring(this.start, this.pos);
    }
    substring(start, end) {
      return this.string.slice(start, end);
    }
    error(message, pos = this.pos) {
      return new ScannerError(`${message} at ${pos + 1}`, pos, this.string);
    }
  };
  var ScannerError = class extends Error {
    constructor(message, pos, str) {
      super(message);
      this.pos = pos;
      this.string = str;
    }
  };
  var scanner_es_default = Scanner;

  // ../node_modules/@emmetio/abbreviation/dist/abbreviation.es.js
  function tokenScanner(tokens) {
    return {
      tokens,
      start: 0,
      pos: 0,
      size: tokens.length
    };
  }
  function peek(scanner) {
    return scanner.tokens[scanner.pos];
  }
  function next(scanner) {
    return scanner.tokens[scanner.pos++];
  }
  function slice(scanner, from = scanner.start, to = scanner.pos) {
    return scanner.tokens.slice(from, to);
  }
  function readable(scanner) {
    return scanner.pos < scanner.size;
  }
  function consume(scanner, test) {
    const token = peek(scanner);
    if (token && test(token)) {
      scanner.pos++;
      return true;
    }
    return false;
  }
  function error(scanner, message, token = peek(scanner)) {
    if (token && token.start != null) {
      message += ` at ${token.start}`;
    }
    const err = new Error(message);
    err["pos"] = token && token.start;
    return err;
  }
  function abbreviation(abbr, options = {}) {
    const scanner = tokenScanner(abbr);
    const result = statements(scanner, options);
    if (readable(scanner)) {
      throw error(scanner, "Unexpected character");
    }
    return result;
  }
  function statements(scanner, options) {
    const result = {
      type: "TokenGroup",
      elements: []
    };
    let ctx = result;
    let node;
    const stack = [];
    while (readable(scanner)) {
      if (node = element(scanner, options) || group(scanner, options)) {
        ctx.elements.push(node);
        if (consume(scanner, isChildOperator)) {
          stack.push(ctx);
          ctx = node;
        } else if (consume(scanner, isSiblingOperator)) {
          continue;
        } else if (consume(scanner, isClimbOperator)) {
          do {
            if (stack.length) {
              ctx = stack.pop();
            }
          } while (consume(scanner, isClimbOperator));
        }
      } else {
        break;
      }
    }
    return result;
  }
  function group(scanner, options) {
    if (consume(scanner, isGroupStart)) {
      const result = statements(scanner, options);
      const token = next(scanner);
      if (isBracket(token, "group", false)) {
        result.repeat = repeater(scanner);
      }
      return result;
    }
  }
  function element(scanner, options) {
    let attr;
    const elem = {
      type: "TokenElement",
      name: void 0,
      attributes: void 0,
      value: void 0,
      repeat: void 0,
      selfClose: false,
      elements: []
    };
    if (elementName(scanner, options)) {
      elem.name = slice(scanner);
    }
    while (readable(scanner)) {
      scanner.start = scanner.pos;
      if (!elem.repeat && !isEmpty(elem) && consume(scanner, isRepeater)) {
        elem.repeat = scanner.tokens[scanner.pos - 1];
      } else if (!elem.value && text(scanner)) {
        elem.value = getText(scanner);
      } else if (attr = shortAttribute(scanner, "id", options) || shortAttribute(scanner, "class", options) || attributeSet(scanner)) {
        if (!elem.attributes) {
          elem.attributes = Array.isArray(attr) ? attr.slice() : [attr];
        } else {
          elem.attributes = elem.attributes.concat(attr);
        }
      } else {
        if (!isEmpty(elem) && consume(scanner, isCloseOperator)) {
          elem.selfClose = true;
          if (!elem.repeat && consume(scanner, isRepeater)) {
            elem.repeat = scanner.tokens[scanner.pos - 1];
          }
        }
        break;
      }
    }
    return !isEmpty(elem) ? elem : void 0;
  }
  function attributeSet(scanner) {
    if (consume(scanner, isAttributeSetStart)) {
      const attributes = [];
      let attr;
      while (readable(scanner)) {
        if (attr = attribute(scanner)) {
          attributes.push(attr);
        } else if (consume(scanner, isAttributeSetEnd)) {
          break;
        } else if (!consume(scanner, isWhiteSpace2)) {
          throw error(scanner, `Unexpected "${peek(scanner).type}" token`);
        }
      }
      return attributes;
    }
  }
  function shortAttribute(scanner, type, options) {
    if (isOperator(peek(scanner), type)) {
      scanner.pos++;
      const attr = {
        name: [createLiteral(type)]
      };
      if (options.jsx && text(scanner)) {
        attr.value = getText(scanner);
        attr.expression = true;
      } else {
        attr.value = literal(scanner) ? slice(scanner) : void 0;
      }
      return attr;
    }
  }
  function attribute(scanner) {
    if (quoted(scanner)) {
      return {
        value: slice(scanner)
      };
    }
    if (literal(scanner, true)) {
      return {
        name: slice(scanner),
        value: consume(scanner, isEquals) && (quoted(scanner) || literal(scanner, true)) ? slice(scanner) : void 0
      };
    }
  }
  function repeater(scanner) {
    return isRepeater(peek(scanner)) ? scanner.tokens[scanner.pos++] : void 0;
  }
  function quoted(scanner) {
    const start = scanner.pos;
    const quote2 = peek(scanner);
    if (isQuote2(quote2)) {
      scanner.pos++;
      while (readable(scanner)) {
        if (isQuote2(next(scanner), quote2.single)) {
          scanner.start = start;
          return true;
        }
      }
      throw error(scanner, "Unclosed quote", quote2);
    }
    return false;
  }
  function literal(scanner, allowBrackets) {
    const start = scanner.pos;
    const brackets = {
      attribute: 0,
      expression: 0,
      group: 0
    };
    while (readable(scanner)) {
      const token = peek(scanner);
      if (brackets.expression) {
        if (isBracket(token, "expression")) {
          brackets[token.context] += token.open ? 1 : -1;
        }
      } else if (isQuote2(token) || isOperator(token) || isWhiteSpace2(token) || isRepeater(token)) {
        break;
      } else if (isBracket(token)) {
        if (!allowBrackets) {
          break;
        }
        if (token.open) {
          brackets[token.context]++;
        } else if (!brackets[token.context]) {
          break;
        } else {
          brackets[token.context]--;
        }
      }
      scanner.pos++;
    }
    if (start !== scanner.pos) {
      scanner.start = start;
      return true;
    }
    return false;
  }
  function elementName(scanner, options) {
    const start = scanner.pos;
    if (options.jsx && consume(scanner, isCapitalizedLiteral)) {
      while (readable(scanner)) {
        const { pos } = scanner;
        if (!consume(scanner, isClassNameOperator) || !consume(scanner, isCapitalizedLiteral)) {
          scanner.pos = pos;
          break;
        }
      }
    }
    while (readable(scanner) && consume(scanner, isElementName)) {
    }
    if (scanner.pos !== start) {
      scanner.start = start;
      return true;
    }
    return false;
  }
  function text(scanner) {
    const start = scanner.pos;
    if (consume(scanner, isTextStart)) {
      let brackets = 0;
      while (readable(scanner)) {
        const token = next(scanner);
        if (isBracket(token, "expression")) {
          if (token.open) {
            brackets++;
          } else if (!brackets) {
            break;
          } else {
            brackets--;
          }
        }
      }
      scanner.start = start;
      return true;
    }
    return false;
  }
  function getText(scanner) {
    let from = scanner.start;
    let to = scanner.pos;
    if (isBracket(scanner.tokens[from], "expression", true)) {
      from++;
    }
    if (isBracket(scanner.tokens[to - 1], "expression", false)) {
      to--;
    }
    return slice(scanner, from, to);
  }
  function isBracket(token, context, isOpen) {
    return Boolean(token && token.type === "Bracket" && (!context || token.context === context) && (isOpen == null || token.open === isOpen));
  }
  function isOperator(token, type) {
    return Boolean(token && token.type === "Operator" && (!type || token.operator === type));
  }
  function isQuote2(token, isSingle) {
    return Boolean(token && token.type === "Quote" && (isSingle == null || token.single === isSingle));
  }
  function isWhiteSpace2(token) {
    return Boolean(token && token.type === "WhiteSpace");
  }
  function isEquals(token) {
    return isOperator(token, "equal");
  }
  function isRepeater(token) {
    return Boolean(token && token.type === "Repeater");
  }
  function isLiteral(token) {
    return token.type === "Literal";
  }
  function isCapitalizedLiteral(token) {
    if (isLiteral(token)) {
      const ch = token.value.charCodeAt(0);
      return ch >= 65 && ch <= 90;
    }
    return false;
  }
  function isElementName(token) {
    return token.type === "Literal" || token.type === "RepeaterNumber" || token.type === "RepeaterPlaceholder";
  }
  function isClassNameOperator(token) {
    return isOperator(token, "class");
  }
  function isAttributeSetStart(token) {
    return isBracket(token, "attribute", true);
  }
  function isAttributeSetEnd(token) {
    return isBracket(token, "attribute", false);
  }
  function isTextStart(token) {
    return isBracket(token, "expression", true);
  }
  function isGroupStart(token) {
    return isBracket(token, "group", true);
  }
  function createLiteral(value) {
    return { type: "Literal", value };
  }
  function isEmpty(elem) {
    return !elem.name && !elem.value && !elem.attributes;
  }
  function isChildOperator(token) {
    return isOperator(token, "child");
  }
  function isSiblingOperator(token) {
    return isOperator(token, "sibling");
  }
  function isClimbOperator(token) {
    return isOperator(token, "climb");
  }
  function isCloseOperator(token) {
    return isOperator(token, "close");
  }
  function escaped(scanner) {
    if (scanner.eat(92)) {
      scanner.start = scanner.pos;
      if (!scanner.eof()) {
        scanner.pos++;
      }
      return true;
    }
    return false;
  }
  function tokenize(source) {
    const scanner = new scanner_es_default(source);
    const result = [];
    const ctx = {
      group: 0,
      attribute: 0,
      expression: 0,
      quote: 0
    };
    let ch = 0;
    let token;
    while (!scanner.eof()) {
      ch = scanner.peek();
      token = getToken(scanner, ctx);
      if (token) {
        result.push(token);
        if (token.type === "Quote") {
          ctx.quote = ch === ctx.quote ? 0 : ch;
        } else if (token.type === "Bracket") {
          ctx[token.context] += token.open ? 1 : -1;
        }
      } else {
        throw scanner.error("Unexpected character");
      }
    }
    return result;
  }
  function getToken(scanner, ctx) {
    return field(scanner, ctx) || repeaterPlaceholder(scanner) || repeaterNumber(scanner) || repeater$1(scanner) || whiteSpace(scanner) || literal$1(scanner, ctx) || operator(scanner) || quote(scanner) || bracket(scanner);
  }
  function literal$1(scanner, ctx) {
    const start = scanner.pos;
    let value = "";
    while (!scanner.eof()) {
      if (escaped(scanner)) {
        value += scanner.current();
        continue;
      }
      const ch = scanner.peek();
      if (ch === ctx.quote || ch === 36 || isAllowedOperator(ch, ctx)) {
        break;
      }
      if (ctx.expression && ch === 125) {
        break;
      }
      if (!ctx.quote && !ctx.expression) {
        if (!ctx.attribute && !isElementName$1(ch)) {
          break;
        }
        if (isAllowedSpace(ch, ctx) || isAllowedRepeater(ch, ctx) || isQuote(ch) || bracketType(ch)) {
          break;
        }
      }
      value += scanner.string[scanner.pos++];
    }
    if (start !== scanner.pos) {
      scanner.start = start;
      return {
        type: "Literal",
        value,
        start,
        end: scanner.pos
      };
    }
  }
  function whiteSpace(scanner) {
    const start = scanner.pos;
    if (scanner.eatWhile(isSpace)) {
      return {
        type: "WhiteSpace",
        start,
        end: scanner.pos,
        value: scanner.substring(start, scanner.pos)
      };
    }
  }
  function quote(scanner) {
    const ch = scanner.peek();
    if (isQuote(ch)) {
      return {
        type: "Quote",
        single: ch === 39,
        start: scanner.pos++,
        end: scanner.pos
      };
    }
  }
  function bracket(scanner) {
    const ch = scanner.peek();
    const context = bracketType(ch);
    if (context) {
      return {
        type: "Bracket",
        open: isOpenBracket(ch),
        context,
        start: scanner.pos++,
        end: scanner.pos
      };
    }
  }
  function operator(scanner) {
    const op = operatorType(scanner.peek());
    if (op) {
      return {
        type: "Operator",
        operator: op,
        start: scanner.pos++,
        end: scanner.pos
      };
    }
  }
  function repeater$1(scanner) {
    const start = scanner.pos;
    if (scanner.eat(42)) {
      scanner.start = scanner.pos;
      let count = 1;
      let implicit = false;
      if (scanner.eatWhile(isNumber)) {
        count = Number(scanner.current());
      } else {
        implicit = true;
      }
      return {
        type: "Repeater",
        count,
        value: 0,
        implicit,
        start,
        end: scanner.pos
      };
    }
  }
  function repeaterPlaceholder(scanner) {
    const start = scanner.pos;
    if (scanner.eat(36) && scanner.eat(35)) {
      return {
        type: "RepeaterPlaceholder",
        value: void 0,
        start,
        end: scanner.pos
      };
    }
    scanner.pos = start;
  }
  function repeaterNumber(scanner) {
    const start = scanner.pos;
    if (scanner.eatWhile(36)) {
      const size = scanner.pos - start;
      let reverse = false;
      let base = 1;
      let parent = 0;
      if (scanner.eat(64)) {
        while (scanner.eat(94)) {
          parent++;
        }
        reverse = scanner.eat(45);
        scanner.start = scanner.pos;
        if (scanner.eatWhile(isNumber)) {
          base = Number(scanner.current());
        }
      }
      scanner.start = start;
      return {
        type: "RepeaterNumber",
        size,
        reverse,
        base,
        parent,
        start,
        end: scanner.pos
      };
    }
  }
  function field(scanner, ctx) {
    const start = scanner.pos;
    if ((ctx.expression || ctx.attribute) && scanner.eat(36) && scanner.eat(123)) {
      scanner.start = scanner.pos;
      let index;
      let name = "";
      if (scanner.eatWhile(isNumber)) {
        index = Number(scanner.current());
        name = scanner.eat(58) ? consumePlaceholder(scanner) : "";
      } else if (isAlpha(scanner.peek())) {
        name = consumePlaceholder(scanner);
      }
      if (scanner.eat(125)) {
        return {
          type: "Field",
          index,
          name,
          start,
          end: scanner.pos
        };
      }
      throw scanner.error("Expecting }");
    }
    scanner.pos = start;
  }
  function consumePlaceholder(stream) {
    const stack = [];
    stream.start = stream.pos;
    while (!stream.eof()) {
      if (stream.eat(123)) {
        stack.push(stream.pos);
      } else if (stream.eat(125)) {
        if (!stack.length) {
          stream.pos--;
          break;
        }
        stack.pop();
      } else {
        stream.pos++;
      }
    }
    if (stack.length) {
      stream.pos = stack.pop();
      throw stream.error(`Expecting }`);
    }
    return stream.current();
  }
  function isAllowedOperator(ch, ctx) {
    const op = operatorType(ch);
    if (!op || ctx.quote || ctx.expression) {
      return false;
    }
    return !ctx.attribute || op === "equal";
  }
  function isAllowedSpace(ch, ctx) {
    return isSpace(ch) && !ctx.expression;
  }
  function isAllowedRepeater(ch, ctx) {
    return ch === 42 && !ctx.attribute && !ctx.expression;
  }
  function bracketType(ch) {
    if (ch === 40 || ch === 41) {
      return "group";
    }
    if (ch === 91 || ch === 93) {
      return "attribute";
    }
    if (ch === 123 || ch === 125) {
      return "expression";
    }
  }
  function operatorType(ch) {
    return ch === 62 && "child" || ch === 43 && "sibling" || ch === 94 && "climb" || ch === 46 && "class" || ch === 35 && "id" || ch === 47 && "close" || ch === 61 && "equal" || void 0;
  }
  function isOpenBracket(ch) {
    return ch === 123 || ch === 91 || ch === 40;
  }
  function isElementName$1(ch) {
    return isAlphaNumericWord(ch) || ch === 45 || ch === 58 || ch === 33;
  }
  var operators = {
    child: ">",
    class: ".",
    climb: "^",
    id: "#",
    equal: "=",
    close: "/",
    sibling: "+"
  };
  var tokenVisitor = {
    Literal(token) {
      return token.value;
    },
    Quote(token) {
      return token.single ? "'" : '"';
    },
    Bracket(token) {
      if (token.context === "attribute") {
        return token.open ? "[" : "]";
      } else if (token.context === "expression") {
        return token.open ? "{" : "}";
      } else {
        return token.open ? "(" : "}";
      }
    },
    Operator(token) {
      return operators[token.operator];
    },
    Field(token, state) {
      if (token.index != null) {
        return token.name ? `\${${token.index}:${token.name}}` : `\${${token.index}`;
      } else if (token.name) {
        return state.getVariable(token.name);
      }
      return "";
    },
    RepeaterPlaceholder(token, state) {
      let repeater2;
      for (let i = state.repeaters.length - 1; i >= 0; i--) {
        if (state.repeaters[i].implicit) {
          repeater2 = state.repeaters[i];
          break;
        }
      }
      state.inserted = true;
      return state.getText(repeater2 && repeater2.value);
    },
    RepeaterNumber(token, state) {
      let value = 1;
      const lastIx = state.repeaters.length - 1;
      const repeater2 = state.repeaters[lastIx];
      if (repeater2) {
        value = token.reverse ? token.base + repeater2.count - repeater2.value - 1 : token.base + repeater2.value;
        if (token.parent) {
          const parentIx = Math.max(0, lastIx - token.parent);
          if (parentIx !== lastIx) {
            const parentRepeater = state.repeaters[parentIx];
            value += repeater2.count * parentRepeater.value;
          }
        }
      }
      let result = String(value);
      while (result.length < token.size) {
        result = "0" + result;
      }
      return result;
    },
    WhiteSpace(token) {
      return token.value;
    }
  };
  function stringify(token, state) {
    if (!tokenVisitor[token.type]) {
      throw new Error(`Unknown token ${token.type}`);
    }
    return tokenVisitor[token.type](token, state);
  }
  var urlRegex = /^((https?:|ftp:|file:)?\/\/|(www|ftp)\.)[^ ]*$/;
  var emailRegex = /^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,5}$/;
  function convert(abbr, options = {}) {
    let textInserted = false;
    let cleanText;
    if (options.text) {
      if (Array.isArray(options.text)) {
        cleanText = options.text.filter((s) => s.trim());
      } else {
        cleanText = options.text;
      }
    }
    const result = {
      type: "Abbreviation",
      children: convertGroup(abbr, {
        inserted: false,
        repeaters: [],
        text: options.text,
        cleanText,
        repeatGuard: options.maxRepeat || Number.POSITIVE_INFINITY,
        getText(pos) {
          var _a;
          textInserted = true;
          let value;
          if (Array.isArray(options.text)) {
            if (pos !== void 0 && pos >= 0 && pos < cleanText.length) {
              return cleanText[pos];
            }
            value = pos !== void 0 ? options.text[pos] : options.text.join("\n");
          } else {
            value = (_a = options.text) !== null && _a !== void 0 ? _a : "";
          }
          return value;
        },
        getVariable(name) {
          const varValue = options.variables && options.variables[name];
          return varValue != null ? varValue : name;
        }
      })
    };
    if (options.text != null && !textInserted) {
      const deepest = deepestNode(last(result.children));
      if (deepest) {
        const text2 = Array.isArray(options.text) ? options.text.join("\n") : options.text;
        insertText(deepest, text2);
        if (deepest.name === "a" && options.href) {
          insertHref(deepest, text2);
        }
      }
    }
    return result;
  }
  function convertStatement(node, state) {
    let result = [];
    if (node.repeat) {
      const original = node.repeat;
      const repeat = Object.assign({}, original);
      repeat.count = repeat.implicit && Array.isArray(state.text) ? state.cleanText.length : repeat.count || 1;
      let items;
      state.repeaters.push(repeat);
      for (let i = 0; i < repeat.count; i++) {
        repeat.value = i;
        node.repeat = repeat;
        items = isGroup(node) ? convertGroup(node, state) : convertElement(node, state);
        if (repeat.implicit && !state.inserted) {
          const target = last(items);
          const deepest = target && deepestNode(target);
          if (deepest) {
            insertText(deepest, state.getText(repeat.value));
          }
        }
        result = result.concat(items);
        if (--state.repeatGuard <= 0) {
          break;
        }
      }
      state.repeaters.pop();
      node.repeat = original;
      if (repeat.implicit) {
        state.inserted = true;
      }
    } else {
      result = result.concat(isGroup(node) ? convertGroup(node, state) : convertElement(node, state));
    }
    return result;
  }
  function convertElement(node, state) {
    let children = [];
    const elem = {
      type: "AbbreviationNode",
      name: node.name && stringifyName(node.name, state),
      value: node.value && stringifyValue(node.value, state),
      attributes: void 0,
      children,
      repeat: node.repeat && Object.assign({}, node.repeat),
      selfClosing: node.selfClose
    };
    let result = [elem];
    for (const child of node.elements) {
      children = children.concat(convertStatement(child, state));
    }
    if (node.attributes) {
      elem.attributes = [];
      for (const attr of node.attributes) {
        elem.attributes.push(convertAttribute(attr, state));
      }
    }
    if (!elem.name && !elem.attributes && elem.value && !elem.value.some(isField)) {
      result = result.concat(children);
    } else {
      elem.children = children;
    }
    return result;
  }
  function convertGroup(node, state) {
    let result = [];
    for (const child of node.elements) {
      result = result.concat(convertStatement(child, state));
    }
    if (node.repeat) {
      result = attachRepeater(result, node.repeat);
    }
    return result;
  }
  function convertAttribute(node, state) {
    let implied = false;
    let isBoolean = false;
    let valueType = node.expression ? "expression" : "raw";
    let value;
    const name = node.name && stringifyName(node.name, state);
    if (name && name[0] === "!") {
      implied = true;
    }
    if (name && name[name.length - 1] === ".") {
      isBoolean = true;
    }
    if (node.value) {
      const tokens = node.value.slice();
      if (isQuote2(tokens[0])) {
        const quote2 = tokens.shift();
        if (tokens.length && last(tokens).type === quote2.type) {
          tokens.pop();
        }
        valueType = quote2.single ? "singleQuote" : "doubleQuote";
      } else if (isBracket(tokens[0], "expression", true)) {
        valueType = "expression";
        tokens.shift();
        if (isBracket(last(tokens), "expression", false)) {
          tokens.pop();
        }
      }
      value = stringifyValue(tokens, state);
    }
    return {
      name: isBoolean || implied ? name.slice(implied ? 1 : 0, isBoolean ? -1 : void 0) : name,
      value,
      boolean: isBoolean,
      implied,
      valueType
    };
  }
  function stringifyName(tokens, state) {
    let str = "";
    for (let i = 0; i < tokens.length; i++) {
      str += stringify(tokens[i], state);
    }
    return str;
  }
  function stringifyValue(tokens, state) {
    const result = [];
    let str = "";
    for (let i = 0, token; i < tokens.length; i++) {
      token = tokens[i];
      if (isField(token)) {
        if (str) {
          result.push(str);
          str = "";
        }
        result.push(token);
      } else {
        str += stringify(token, state);
      }
    }
    if (str) {
      result.push(str);
    }
    return result;
  }
  function isGroup(node) {
    return node.type === "TokenGroup";
  }
  function isField(token) {
    return typeof token === "object" && token.type === "Field" && token.index != null;
  }
  function last(arr) {
    return arr[arr.length - 1];
  }
  function deepestNode(node) {
    return node.children.length ? deepestNode(last(node.children)) : node;
  }
  function insertText(node, text2) {
    if (node.value) {
      const lastToken = last(node.value);
      if (typeof lastToken === "string") {
        node.value[node.value.length - 1] += text2;
      } else {
        node.value.push(text2);
      }
    } else {
      node.value = [text2];
    }
  }
  function insertHref(node, text2) {
    var _a;
    let href = "";
    if (urlRegex.test(text2)) {
      href = text2;
      if (!/\w+:/.test(href) && !href.startsWith("//")) {
        href = `http://${href}`;
      }
    } else if (emailRegex.test(text2)) {
      href = `mailto:${text2}`;
    }
    const hrefAttribute = (_a = node.attributes) === null || _a === void 0 ? void 0 : _a.find((attr) => attr.name === "href");
    if (!hrefAttribute) {
      node.attributes = [{ name: "href", value: [href], valueType: "doubleQuote" }];
    } else if (!hrefAttribute.value) {
      hrefAttribute.value = [href];
    }
  }
  function attachRepeater(items, repeater2) {
    for (const item of items) {
      if (!item.repeat) {
        item.repeat = Object.assign({}, repeater2);
      }
    }
    return items;
  }
  function parseAbbreviation(abbr, options) {
    try {
      const tokens = typeof abbr === "string" ? tokenize(abbr) : abbr;
      return convert(abbreviation(tokens, options), options);
    } catch (err) {
      if (err instanceof ScannerError && typeof abbr === "string") {
        err.message += `
${abbr}
${"-".repeat(err.pos)}^`;
      }
      throw err;
    }
  }
  var abbreviation_es_default = parseAbbreviation;

  // ../node_modules/@emmetio/css-abbreviation/dist/css-abbreviation.es.js
  function tokenize2(abbr, isValue2) {
    let brackets = 0;
    let token;
    const scanner = new scanner_es_default(abbr);
    const tokens = [];
    while (!scanner.eof()) {
      token = getToken2(scanner, brackets === 0 && !isValue2);
      if (!token) {
        throw scanner.error("Unexpected character");
      }
      if (token.type === "Bracket") {
        if (!brackets && token.open) {
          mergeTokens(scanner, tokens);
        }
        brackets += token.open ? 1 : -1;
        if (brackets < 0) {
          throw scanner.error("Unexpected bracket", token.start);
        }
      }
      tokens.push(token);
      if (shouldConsumeDashAfter(token) && (token = operator2(scanner))) {
        tokens.push(token);
      }
    }
    return tokens;
  }
  function getToken2(scanner, short) {
    return field2(scanner) || numberValue(scanner) || colorValue(scanner) || stringValue(scanner) || bracket2(scanner) || operator2(scanner) || whiteSpace2(scanner) || literal2(scanner, short);
  }
  function field2(scanner) {
    const start = scanner.pos;
    if (scanner.eat(36) && scanner.eat(123)) {
      scanner.start = scanner.pos;
      let index;
      let name = "";
      if (scanner.eatWhile(isNumber)) {
        index = Number(scanner.current());
        name = scanner.eat(58) ? consumePlaceholder2(scanner) : "";
      } else if (isAlpha(scanner.peek())) {
        name = consumePlaceholder2(scanner);
      }
      if (scanner.eat(125)) {
        return {
          type: "Field",
          index,
          name,
          start,
          end: scanner.pos
        };
      }
      throw scanner.error("Expecting }");
    }
    scanner.pos = start;
  }
  function consumePlaceholder2(stream) {
    const stack = [];
    stream.start = stream.pos;
    while (!stream.eof()) {
      if (stream.eat(123)) {
        stack.push(stream.pos);
      } else if (stream.eat(125)) {
        if (!stack.length) {
          stream.pos--;
          break;
        }
        stack.pop();
      } else {
        stream.pos++;
      }
    }
    if (stack.length) {
      stream.pos = stack.pop();
      throw stream.error(`Expecting }`);
    }
    return stream.current();
  }
  function literal2(scanner, short) {
    const start = scanner.pos;
    if (scanner.eat(isIdentPrefix)) {
      scanner.eatWhile(start ? isKeyword : isLiteral2);
    } else if (scanner.eat(isAlphaWord)) {
      scanner.eatWhile(short ? isLiteral2 : isKeyword);
    } else {
      scanner.eat(46);
      scanner.eatWhile(isLiteral2);
    }
    if (start !== scanner.pos) {
      scanner.start = start;
      return createLiteral2(scanner, scanner.start = start);
    }
  }
  function createLiteral2(scanner, start = scanner.start, end = scanner.pos) {
    return {
      type: "Literal",
      value: scanner.substring(start, end),
      start,
      end
    };
  }
  function numberValue(scanner) {
    const start = scanner.pos;
    if (consumeNumber(scanner)) {
      scanner.start = start;
      const rawValue = scanner.current();
      scanner.start = scanner.pos;
      scanner.eat(37) || scanner.eatWhile(isAlphaWord);
      return {
        type: "NumberValue",
        value: Number(rawValue),
        rawValue,
        unit: scanner.current(),
        start,
        end: scanner.pos
      };
    }
  }
  function stringValue(scanner) {
    const ch = scanner.peek();
    const start = scanner.pos;
    let finished = false;
    if (isQuote(ch)) {
      scanner.pos++;
      while (!scanner.eof()) {
        if (scanner.eat(ch)) {
          finished = true;
          break;
        } else {
          scanner.pos++;
        }
      }
      scanner.start = start;
      return {
        type: "StringValue",
        value: scanner.substring(start + 1, scanner.pos - (finished ? 1 : 0)),
        quote: ch === 39 ? "single" : "double",
        start,
        end: scanner.pos
      };
    }
  }
  function colorValue(scanner) {
    const start = scanner.pos;
    if (scanner.eat(35)) {
      const valueStart = scanner.pos;
      let color2 = "";
      let alpha = "";
      if (scanner.eatWhile(isHex)) {
        color2 = scanner.substring(valueStart, scanner.pos);
        alpha = colorAlpha(scanner);
      } else if (scanner.eat(116)) {
        color2 = "0";
        alpha = colorAlpha(scanner) || "0";
      } else {
        alpha = colorAlpha(scanner);
      }
      if (color2 || alpha || scanner.eof()) {
        const { r, g, b, a } = parseColor(color2, alpha);
        return {
          type: "ColorValue",
          r,
          g,
          b,
          a,
          raw: scanner.substring(start + 1, scanner.pos),
          start,
          end: scanner.pos
        };
      } else {
        return createLiteral2(scanner, start);
      }
    }
    scanner.pos = start;
  }
  function colorAlpha(scanner) {
    const start = scanner.pos;
    if (scanner.eat(46)) {
      scanner.start = start;
      if (scanner.eatWhile(isNumber)) {
        return scanner.current();
      }
      return "1";
    }
    return "";
  }
  function whiteSpace2(scanner) {
    const start = scanner.pos;
    if (scanner.eatWhile(isSpace)) {
      return {
        type: "WhiteSpace",
        start,
        end: scanner.pos
      };
    }
  }
  function bracket2(scanner) {
    const ch = scanner.peek();
    if (isBracket2(ch)) {
      return {
        type: "Bracket",
        open: ch === 40,
        start: scanner.pos++,
        end: scanner.pos
      };
    }
  }
  function operator2(scanner) {
    const op = operatorType2(scanner.peek());
    if (op) {
      return {
        type: "Operator",
        operator: op,
        start: scanner.pos++,
        end: scanner.pos
      };
    }
  }
  function consumeNumber(stream) {
    const start = stream.pos;
    stream.eat(45);
    const afterNegative = stream.pos;
    const hasDecimal = stream.eatWhile(isNumber);
    const prevPos = stream.pos;
    if (stream.eat(46)) {
      const hasFloat = stream.eatWhile(isNumber);
      if (!hasDecimal && !hasFloat) {
        stream.pos = prevPos;
      }
    }
    if (stream.pos === afterNegative) {
      stream.pos = start;
    }
    return stream.pos !== start;
  }
  function isIdentPrefix(code2) {
    return code2 === 64 || code2 === 36;
  }
  function operatorType2(ch) {
    return ch === 43 && "+" || ch === 33 && "!" || ch === 44 && "," || ch === 58 && ":" || ch === 45 && "-" || void 0;
  }
  function isHex(code2) {
    return isNumber(code2) || isAlpha(code2, 65, 70);
  }
  function isKeyword(code2) {
    return isAlphaNumericWord(code2) || code2 === 45;
  }
  function isBracket2(code2) {
    return code2 === 40 || code2 === 41;
  }
  function isLiteral2(code2) {
    return isAlphaWord(code2) || code2 === 37 || code2 === 47;
  }
  function parseColor(value, alpha) {
    let r = "0";
    let g = "0";
    let b = "0";
    let a = Number(alpha != null && alpha !== "" ? alpha : 1);
    if (value === "t") {
      a = 0;
    } else {
      switch (value.length) {
        case 0:
          break;
        case 1:
          r = g = b = value + value;
          break;
        case 2:
          r = g = b = value;
          break;
        case 3:
          r = value[0] + value[0];
          g = value[1] + value[1];
          b = value[2] + value[2];
          break;
        default:
          value += value;
          r = value.slice(0, 2);
          g = value.slice(2, 4);
          b = value.slice(4, 6);
      }
    }
    return {
      r: parseInt(r, 16),
      g: parseInt(g, 16),
      b: parseInt(b, 16),
      a
    };
  }
  function shouldConsumeDashAfter(token) {
    return token.type === "ColorValue" || token.type === "NumberValue" && !token.unit;
  }
  function mergeTokens(scanner, tokens) {
    let start = 0;
    let end = 0;
    while (tokens.length) {
      const token = last2(tokens);
      if (token.type === "Literal" || token.type === "NumberValue") {
        start = token.start;
        if (!end) {
          end = token.end;
        }
        tokens.pop();
      } else {
        break;
      }
    }
    if (start !== end) {
      tokens.push(createLiteral2(scanner, start, end));
    }
  }
  function last2(arr) {
    return arr[arr.length - 1];
  }
  function tokenScanner2(tokens) {
    return {
      tokens,
      start: 0,
      pos: 0,
      size: tokens.length
    };
  }
  function peek2(scanner) {
    return scanner.tokens[scanner.pos];
  }
  function readable2(scanner) {
    return scanner.pos < scanner.size;
  }
  function consume2(scanner, test) {
    if (test(peek2(scanner))) {
      scanner.pos++;
      return true;
    }
    return false;
  }
  function error2(scanner, message, token = peek2(scanner)) {
    if (token && token.start != null) {
      message += ` at ${token.start}`;
    }
    const err = new Error(message);
    err["pos"] = token && token.start;
    return err;
  }
  function parser(tokens, options = {}) {
    const scanner = tokenScanner2(tokens);
    const result = [];
    let property2;
    while (readable2(scanner)) {
      if (property2 = consumeProperty(scanner, options)) {
        result.push(property2);
      } else if (!consume2(scanner, isSiblingOperator2)) {
        throw error2(scanner, "Unexpected token");
      }
    }
    return result;
  }
  function consumeProperty(scanner, options) {
    let name;
    let important = false;
    let valueFragment;
    const value = [];
    const token = peek2(scanner);
    const valueMode = !!options.value;
    if (!valueMode && isLiteral$1(token) && !isFunctionStart(scanner)) {
      scanner.pos++;
      name = token.value;
      consume2(scanner, isValueDelimiter);
    }
    if (valueMode) {
      consume2(scanner, isWhiteSpace3);
    }
    while (readable2(scanner)) {
      if (consume2(scanner, isImportant)) {
        important = true;
      } else if (valueFragment = consumeValue(scanner, valueMode)) {
        value.push(valueFragment);
      } else if (!consume2(scanner, isFragmentDelimiter)) {
        break;
      }
    }
    if (name || value.length || important) {
      return { name, value, important };
    }
  }
  function consumeValue(scanner, inArgument) {
    const result = [];
    let token;
    let args;
    while (readable2(scanner)) {
      token = peek2(scanner);
      if (isValue(token)) {
        scanner.pos++;
        if (isLiteral$1(token) && (args = consumeArguments(scanner))) {
          result.push({
            type: "FunctionCall",
            name: token.value,
            arguments: args
          });
        } else {
          result.push(token);
        }
      } else if (isValueDelimiter(token) || inArgument && isWhiteSpace3(token)) {
        scanner.pos++;
      } else {
        break;
      }
    }
    return result.length ? { type: "CSSValue", value: result } : void 0;
  }
  function consumeArguments(scanner) {
    const start = scanner.pos;
    if (consume2(scanner, isOpenBracket2)) {
      const args = [];
      let value;
      while (readable2(scanner) && !consume2(scanner, isCloseBracket)) {
        if (value = consumeValue(scanner, true)) {
          args.push(value);
        } else if (!consume2(scanner, isWhiteSpace3) && !consume2(scanner, isArgumentDelimiter)) {
          throw error2(scanner, "Unexpected token");
        }
      }
      scanner.start = start;
      return args;
    }
  }
  function isLiteral$1(token) {
    return token && token.type === "Literal";
  }
  function isBracket$1(token, open) {
    return token && token.type === "Bracket" && (open == null || token.open === open);
  }
  function isOpenBracket2(token) {
    return isBracket$1(token, true);
  }
  function isCloseBracket(token) {
    return isBracket$1(token, false);
  }
  function isWhiteSpace3(token) {
    return token && token.type === "WhiteSpace";
  }
  function isOperator2(token, operator3) {
    return token && token.type === "Operator" && (!operator3 || token.operator === operator3);
  }
  function isSiblingOperator2(token) {
    return isOperator2(token, "+");
  }
  function isArgumentDelimiter(token) {
    return isOperator2(token, ",");
  }
  function isFragmentDelimiter(token) {
    return isArgumentDelimiter(token);
  }
  function isImportant(token) {
    return isOperator2(token, "!");
  }
  function isValue(token) {
    return token.type === "StringValue" || token.type === "ColorValue" || token.type === "NumberValue" || token.type === "Literal" || token.type === "Field";
  }
  function isValueDelimiter(token) {
    return isOperator2(token, ":") || isOperator2(token, "-");
  }
  function isFunctionStart(scanner) {
    const t1 = scanner.tokens[scanner.pos];
    const t2 = scanner.tokens[scanner.pos + 1];
    return t1 && t2 && isLiteral$1(t1) && t2.type === "Bracket";
  }
  function parse(abbr, options) {
    try {
      const tokens = typeof abbr === "string" ? tokenize2(abbr, options && options.value) : abbr;
      return parser(tokens, options);
    } catch (err) {
      if (err instanceof ScannerError && typeof abbr === "string") {
        err.message += `
${abbr}
${"-".repeat(err.pos)}^`;
      }
      throw err;
    }
  }
  var css_abbreviation_es_default = parse;

  // emmet.es.js
  function mergeAttributes(node, config) {
    if (!node.attributes) {
      return;
    }
    const attributes = [];
    const lookup = {};
    for (const attr of node.attributes) {
      if (attr.name) {
        const attrName2 = attr.name;
        if (attrName2 in lookup) {
          const prev = lookup[attrName2];
          if (attrName2 === "class") {
            prev.value = mergeValue(prev.value, attr.value, " ");
          } else {
            mergeDeclarations(prev, attr, config);
          }
        } else {
          attributes.push(lookup[attrName2] = Object.assign({}, attr));
        }
      } else {
        attributes.push(attr);
      }
    }
    node.attributes = attributes;
  }
  function mergeValue(prev, next2, glue) {
    if (prev && next2) {
      if (prev.length && glue) {
        append(prev, glue);
      }
      for (const t of next2) {
        append(prev, t);
      }
      return prev;
    }
    const result = prev || next2;
    return result && result.slice();
  }
  function mergeDeclarations(dest, src, config) {
    dest.name = src.name;
    if (!config.options["output.reverseAttributes"]) {
      dest.value = src.value;
    }
    if (!dest.implied) {
      dest.implied = src.implied;
    }
    if (!dest.boolean) {
      dest.boolean = src.boolean;
    }
    if (dest.valueType !== "expression") {
      dest.valueType = src.valueType;
    }
    return dest;
  }
  function append(tokens, value) {
    const lastIx = tokens.length - 1;
    if (typeof tokens[lastIx] === "string" && typeof value === "string") {
      tokens[lastIx] += value;
    } else {
      tokens.push(value);
    }
  }
  function walk(node, fn, state) {
    const ancestors = [node];
    const callback = (ctx) => {
      fn(ctx, ancestors, state);
      ancestors.push(ctx);
      ctx.children.forEach(callback);
      ancestors.pop();
    };
    node.children.forEach(callback);
  }
  function findDeepest(node) {
    let parent;
    while (node.children.length) {
      parent = node;
      node = node.children[node.children.length - 1];
    }
    return { parent, node };
  }
  function isNode(node) {
    return node.type === "AbbreviationNode";
  }
  function resolveSnippets(abbr, config) {
    const stack = [];
    const reversed = config.options["output.reverseAttributes"];
    const resolve = (child) => {
      const snippet = child.name && config.snippets[child.name];
      if (!snippet || stack.includes(snippet)) {
        return null;
      }
      const snippetAbbr = abbreviation_es_default(snippet, config);
      stack.push(snippet);
      walkResolve(snippetAbbr, resolve);
      stack.pop();
      for (const topNode of snippetAbbr.children) {
        if (child.attributes) {
          const from = topNode.attributes || [];
          const to = child.attributes || [];
          topNode.attributes = reversed ? to.concat(from) : from.concat(to);
        }
        mergeNodes(child, topNode);
      }
      return snippetAbbr;
    };
    walkResolve(abbr, resolve);
    return abbr;
  }
  function walkResolve(node, resolve, config) {
    let children = [];
    for (const child of node.children) {
      const resolved = resolve(child);
      if (resolved) {
        children = children.concat(resolved.children);
        const deepest = findDeepest(resolved);
        if (isNode(deepest.node)) {
          deepest.node.children = deepest.node.children.concat(walkResolve(child, resolve));
        }
      } else {
        children.push(child);
        child.children = walkResolve(child, resolve);
      }
    }
    return node.children = children;
  }
  function mergeNodes(from, to) {
    if (from.selfClosing) {
      to.selfClosing = true;
    }
    if (from.value != null) {
      to.value = from.value;
    }
    if (from.repeat) {
      to.repeat = from.repeat;
    }
  }
  function createOutputStream(options, level = 0) {
    return {
      options,
      value: "",
      level,
      offset: 0,
      line: 0,
      column: 0
    };
  }
  function push(stream, text2) {
    const processText = stream.options["output.text"];
    _push(stream, processText(text2, stream.offset, stream.line, stream.column));
  }
  function pushString(stream, value) {
    const lines = splitByLines(value);
    for (let i = 0, il = lines.length - 1; i <= il; i++) {
      push(stream, lines[i]);
      if (i !== il) {
        pushNewline(stream, true);
      }
    }
  }
  function pushNewline(stream, indent) {
    const baseIndent = stream.options["output.baseIndent"];
    const newline = stream.options["output.newline"];
    push(stream, newline + baseIndent);
    stream.line++;
    stream.column = baseIndent.length;
    if (indent) {
      pushIndent(stream, indent === true ? stream.level : indent);
    }
  }
  function pushIndent(stream, size = stream.level) {
    const indent = stream.options["output.indent"];
    push(stream, indent.repeat(Math.max(size, 0)));
  }
  function pushField(stream, index, placeholder) {
    const field4 = stream.options["output.field"];
    _push(stream, field4(index, placeholder, stream.offset, stream.line, stream.column));
  }
  function tagName(name, config) {
    return strCase(name, config.options["output.tagCase"]);
  }
  function attrName(name, config) {
    return strCase(name, config.options["output.attributeCase"]);
  }
  function attrQuote(attr, config, isOpen) {
    if (attr.valueType === "expression") {
      return isOpen ? "{" : "}";
    }
    return config.options["output.attributeQuotes"] === "single" ? "'" : '"';
  }
  function isBooleanAttribute(attr, config) {
    return attr.boolean || config.options["output.booleanAttributes"].includes((attr.name || "").toLowerCase());
  }
  function selfClose(config) {
    switch (config.options["output.selfClosingStyle"]) {
      case "xhtml":
        return " /";
      case "xml":
        return "/";
      default:
        return "";
    }
  }
  function isInline(node, config) {
    if (typeof node === "string") {
      return config.options.inlineElements.includes(node.toLowerCase());
    }
    return node.name ? isInline(node.name, config) : Boolean(node.value && !node.attributes);
  }
  function splitByLines(text2) {
    return text2.split(/\r\n|\r|\n/g);
  }
  function _push(stream, text2) {
    stream.value += text2;
    stream.offset += text2.length;
    stream.column += text2.length;
  }
  function strCase(str, type) {
    if (type) {
      return type === "upper" ? str.toUpperCase() : str.toLowerCase();
    }
    return str;
  }
  var elementMap = {
    p: "span",
    ul: "li",
    ol: "li",
    table: "tr",
    tr: "td",
    tbody: "tr",
    thead: "tr",
    tfoot: "tr",
    colgroup: "col",
    select: "option",
    optgroup: "option",
    audio: "source",
    video: "source",
    object: "param",
    map: "area"
  };
  function implicitTag(node, ancestors, config) {
    if (!node.name && node.attributes) {
      resolveImplicitTag(node, ancestors, config);
    }
  }
  function resolveImplicitTag(node, ancestors, config) {
    const parent = getParentElement(ancestors);
    const contextName = config.context ? config.context.name : "";
    const parentName = lowercase(parent ? parent.name : contextName);
    node.name = elementMap[parentName] || (isInline(parentName, config) ? "span" : "div");
  }
  function lowercase(str) {
    return (str || "").toLowerCase();
  }
  function getParentElement(ancestors) {
    for (let i = ancestors.length - 1; i >= 0; i--) {
      const elem = ancestors[i];
      if (isNode(elem)) {
        return elem;
      }
    }
  }
  var latin = {
    "common": ["lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipisicing", "elit"],
    "words": [
      "exercitationem",
      "perferendis",
      "perspiciatis",
      "laborum",
      "eveniet",
      "sunt",
      "iure",
      "nam",
      "nobis",
      "eum",
      "cum",
      "officiis",
      "excepturi",
      "odio",
      "consectetur",
      "quasi",
      "aut",
      "quisquam",
      "vel",
      "eligendi",
      "itaque",
      "non",
      "odit",
      "tempore",
      "quaerat",
      "dignissimos",
      "facilis",
      "neque",
      "nihil",
      "expedita",
      "vitae",
      "vero",
      "ipsum",
      "nisi",
      "animi",
      "cumque",
      "pariatur",
      "velit",
      "modi",
      "natus",
      "iusto",
      "eaque",
      "sequi",
      "illo",
      "sed",
      "ex",
      "et",
      "voluptatibus",
      "tempora",
      "veritatis",
      "ratione",
      "assumenda",
      "incidunt",
      "nostrum",
      "placeat",
      "aliquid",
      "fuga",
      "provident",
      "praesentium",
      "rem",
      "necessitatibus",
      "suscipit",
      "adipisci",
      "quidem",
      "possimus",
      "voluptas",
      "debitis",
      "sint",
      "accusantium",
      "unde",
      "sapiente",
      "voluptate",
      "qui",
      "aspernatur",
      "laudantium",
      "soluta",
      "amet",
      "quo",
      "aliquam",
      "saepe",
      "culpa",
      "libero",
      "ipsa",
      "dicta",
      "reiciendis",
      "nesciunt",
      "doloribus",
      "autem",
      "impedit",
      "minima",
      "maiores",
      "repudiandae",
      "ipsam",
      "obcaecati",
      "ullam",
      "enim",
      "totam",
      "delectus",
      "ducimus",
      "quis",
      "voluptates",
      "dolores",
      "molestiae",
      "harum",
      "dolorem",
      "quia",
      "voluptatem",
      "molestias",
      "magni",
      "distinctio",
      "omnis",
      "illum",
      "dolorum",
      "voluptatum",
      "ea",
      "quas",
      "quam",
      "corporis",
      "quae",
      "blanditiis",
      "atque",
      "deserunt",
      "laboriosam",
      "earum",
      "consequuntur",
      "hic",
      "cupiditate",
      "quibusdam",
      "accusamus",
      "ut",
      "rerum",
      "error",
      "minus",
      "eius",
      "ab",
      "ad",
      "nemo",
      "fugit",
      "officia",
      "at",
      "in",
      "id",
      "quos",
      "reprehenderit",
      "numquam",
      "iste",
      "fugiat",
      "sit",
      "inventore",
      "beatae",
      "repellendus",
      "magnam",
      "recusandae",
      "quod",
      "explicabo",
      "doloremque",
      "aperiam",
      "consequatur",
      "asperiores",
      "commodi",
      "optio",
      "dolor",
      "labore",
      "temporibus",
      "repellat",
      "veniam",
      "architecto",
      "est",
      "esse",
      "mollitia",
      "nulla",
      "a",
      "similique",
      "eos",
      "alias",
      "dolore",
      "tenetur",
      "deleniti",
      "porro",
      "facere",
      "maxime",
      "corrupti"
    ]
  };
  var ru = {
    "common": ["\u0434\u0430\u043B\u0435\u043A\u043E-\u0434\u0430\u043B\u0435\u043A\u043E", "\u0437\u0430", "\u0441\u043B\u043E\u0432\u0435\u0441\u043D\u044B\u043C\u0438", "\u0433\u043E\u0440\u0430\u043C\u0438", "\u0432 \u0441\u0442\u0440\u0430\u043D\u0435", "\u0433\u043B\u0430\u0441\u043D\u044B\u0445", "\u0438 \u0441\u043E\u0433\u043B\u0430\u0441\u043D\u044B\u0445", "\u0436\u0438\u0432\u0443\u0442", "\u0440\u044B\u0431\u043D\u044B\u0435", "\u0442\u0435\u043A\u0441\u0442\u044B"],
    "words": [
      "\u0432\u0434\u0430\u043B\u0438",
      "\u043E\u0442 \u0432\u0441\u0435\u0445",
      "\u043E\u043D\u0438",
      "\u0431\u0443\u043A\u0432\u0435\u043D\u043D\u044B\u0445",
      "\u0434\u043E\u043C\u0430\u0445",
      "\u043D\u0430 \u0431\u0435\u0440\u0435\u0433\u0443",
      "\u0441\u0435\u043C\u0430\u043D\u0442\u0438\u043A\u0430",
      "\u0431\u043E\u043B\u044C\u0448\u043E\u0433\u043E",
      "\u044F\u0437\u044B\u043A\u043E\u0432\u043E\u0433\u043E",
      "\u043E\u043A\u0435\u0430\u043D\u0430",
      "\u043C\u0430\u043B\u0435\u043D\u044C\u043A\u0438\u0439",
      "\u0440\u0443\u0447\u0435\u0435\u043A",
      "\u0434\u0430\u043B\u044C",
      "\u0436\u0443\u0440\u0447\u0438\u0442",
      "\u043F\u043E \u0432\u0441\u0435\u0439",
      "\u043E\u0431\u0435\u0441\u043F\u0435\u0447\u0438\u0432\u0430\u0435\u0442",
      "\u0435\u0435",
      "\u0432\u0441\u0435\u043C\u0438",
      "\u043D\u0435\u043E\u0431\u0445\u043E\u0434\u0438\u043C\u044B\u043C\u0438",
      "\u043F\u0440\u0430\u0432\u0438\u043B\u0430\u043C\u0438",
      "\u044D\u0442\u0430",
      "\u043F\u0430\u0440\u0430\u0434\u0438\u0433\u043C\u0430\u0442\u0438\u0447\u0435\u0441\u043A\u0430\u044F",
      "\u0441\u0442\u0440\u0430\u043D\u0430",
      "\u043A\u043E\u0442\u043E\u0440\u043E\u0439",
      "\u0436\u0430\u0440\u0435\u043D\u043D\u044B\u0435",
      "\u043F\u0440\u0435\u0434\u043B\u043E\u0436\u0435\u043D\u0438\u044F",
      "\u0437\u0430\u043B\u0435\u0442\u0430\u044E\u0442",
      "\u043F\u0440\u044F\u043C\u043E",
      "\u0440\u043E\u0442",
      "\u0434\u0430\u0436\u0435",
      "\u0432\u0441\u0435\u043C\u043E\u0433\u0443\u0449\u0430\u044F",
      "\u043F\u0443\u043D\u043A\u0442\u0443\u0430\u0446\u0438\u044F",
      "\u043D\u0435",
      "\u0438\u043C\u0435\u0435\u0442",
      "\u0432\u043B\u0430\u0441\u0442\u0438",
      "\u043D\u0430\u0434",
      "\u0440\u044B\u0431\u043D\u044B\u043C\u0438",
      "\u0442\u0435\u043A\u0441\u0442\u0430\u043C\u0438",
      "\u0432\u0435\u0434\u0443\u0449\u0438\u043C\u0438",
      "\u0431\u0435\u0437\u043E\u0440\u0444\u043E\u0433\u0440\u0430\u0444\u0438\u0447\u043D\u044B\u0439",
      "\u043E\u0431\u0440\u0430\u0437",
      "\u0436\u0438\u0437\u043D\u0438",
      "\u043E\u0434\u043D\u0430\u0436\u0434\u044B",
      "\u043E\u0434\u043D\u0430",
      "\u043C\u0430\u043B\u0435\u043D\u044C\u043A\u0430\u044F",
      "\u0441\u0442\u0440\u043E\u0447\u043A\u0430",
      "\u0440\u044B\u0431\u043D\u043E\u0433\u043E",
      "\u0442\u0435\u043A\u0441\u0442\u0430",
      "\u0438\u043C\u0435\u043D\u0438",
      "lorem",
      "ipsum",
      "\u0440\u0435\u0448\u0438\u043B\u0430",
      "\u0432\u044B\u0439\u0442\u0438",
      "\u0431\u043E\u043B\u044C\u0448\u043E\u0439",
      "\u043C\u0438\u0440",
      "\u0433\u0440\u0430\u043C\u043C\u0430\u0442\u0438\u043A\u0438",
      "\u0432\u0435\u043B\u0438\u043A\u0438\u0439",
      "\u043E\u043A\u0441\u043C\u043E\u043A\u0441",
      "\u043F\u0440\u0435\u0434\u0443\u043F\u0440\u0435\u0436\u0434\u0430\u043B",
      "\u043E",
      "\u0437\u043B\u044B\u0445",
      "\u0437\u0430\u043F\u044F\u0442\u044B\u0445",
      "\u0434\u0438\u043A\u0438\u0445",
      "\u0437\u043D\u0430\u043A\u0430\u0445",
      "\u0432\u043E\u043F\u0440\u043E\u0441\u0430",
      "\u043A\u043E\u0432\u0430\u0440\u043D\u044B\u0445",
      "\u0442\u043E\u0447\u043A\u0430\u0445",
      "\u0437\u0430\u043F\u044F\u0442\u043E\u0439",
      "\u043D\u043E",
      "\u0442\u0435\u043A\u0441\u0442",
      "\u0434\u0430\u043B",
      "\u0441\u0431\u0438\u0442\u044C",
      "\u0441\u0435\u0431\u044F",
      "\u0442\u043E\u043B\u043A\u0443",
      "\u043E\u043D",
      "\u0441\u043E\u0431\u0440\u0430\u043B",
      "\u0441\u0435\u043C\u044C",
      "\u0441\u0432\u043E\u0438\u0445",
      "\u0437\u0430\u0433\u043B\u0430\u0432\u043D\u044B\u0445",
      "\u0431\u0443\u043A\u0432",
      "\u043F\u043E\u0434\u043F\u043E\u044F\u0441\u0430\u043B",
      "\u0438\u043D\u0438\u0446\u0438\u0430\u043B",
      "\u0437\u0430",
      "\u043F\u043E\u044F\u0441",
      "\u043F\u0443\u0441\u0442\u0438\u043B\u0441\u044F",
      "\u0434\u043E\u0440\u043E\u0433\u0443",
      "\u0432\u0437\u043E\u0431\u0440\u0430\u0432\u0448\u0438\u0441\u044C",
      "\u043F\u0435\u0440\u0432\u0443\u044E",
      "\u0432\u0435\u0440\u0448\u0438\u043D\u0443",
      "\u043A\u0443\u0440\u0441\u0438\u0432\u043D\u044B\u0445",
      "\u0433\u043E\u0440",
      "\u0431\u0440\u043E\u0441\u0438\u043B",
      "\u043F\u043E\u0441\u043B\u0435\u0434\u043D\u0438\u0439",
      "\u0432\u0437\u0433\u043B\u044F\u0434",
      "\u043D\u0430\u0437\u0430\u0434",
      "\u0441\u0438\u043B\u0443\u044D\u0442",
      "\u0441\u0432\u043E\u0435\u0433\u043E",
      "\u0440\u043E\u0434\u043D\u043E\u0433\u043E",
      "\u0433\u043E\u0440\u043E\u0434\u0430",
      "\u0431\u0443\u043A\u0432\u043E\u0433\u0440\u0430\u0434",
      "\u0437\u0430\u0433\u043E\u043B\u043E\u0432\u043E\u043A",
      "\u0434\u0435\u0440\u0435\u0432\u043D\u0438",
      "\u0430\u043B\u0444\u0430\u0432\u0438\u0442",
      "\u043F\u043E\u0434\u0437\u0430\u0433\u043E\u043B\u043E\u0432\u043E\u043A",
      "\u0441\u0432\u043E\u0435\u0433\u043E",
      "\u043F\u0435\u0440\u0435\u0443\u043B\u043A\u0430",
      "\u0433\u0440\u0443\u0441\u0442\u043D\u044B\u0439",
      "\u0440\u0435\u0442\u043E\u0440\u0438\u0447\u0435\u0441\u043A\u0438\u0439",
      "\u0432\u043E\u043F\u0440\u043E\u0441",
      "\u0441\u043A\u0430\u0442\u0438\u043B\u0441\u044F",
      "\u0435\u0433\u043E",
      "\u0449\u0435\u043A\u0435",
      "\u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u043B",
      "\u0441\u0432\u043E\u0439",
      "\u043F\u0443\u0442\u044C",
      "\u0434\u043E\u0440\u043E\u0433\u0435",
      "\u0432\u0441\u0442\u0440\u0435\u0442\u0438\u043B",
      "\u0440\u0443\u043A\u043E\u043F\u0438\u0441\u044C",
      "\u043E\u043D\u0430",
      "\u043F\u0440\u0435\u0434\u0443\u043F\u0440\u0435\u0434\u0438\u043B\u0430",
      "\u043C\u043E\u0435\u0439",
      "\u0432\u0441\u0435",
      "\u043F\u0435\u0440\u0435\u043F\u0438\u0441\u044B\u0432\u0430\u0435\u0442\u0441\u044F",
      "\u043D\u0435\u0441\u043A\u043E\u043B\u044C\u043A\u043E",
      "\u0440\u0430\u0437",
      "\u0435\u0434\u0438\u043D\u0441\u0442\u0432\u0435\u043D\u043D\u043E\u0435",
      "\u0447\u0442\u043E",
      "\u043C\u0435\u043D\u044F",
      "\u043E\u0441\u0442\u0430\u043B\u043E\u0441\u044C",
      "\u044D\u0442\u043E",
      "\u043F\u0440\u0438\u0441\u0442\u0430\u0432\u043A\u0430",
      "\u0432\u043E\u0437\u0432\u0440\u0430\u0449\u0430\u0439\u0441\u044F",
      "\u0442\u044B",
      "\u043B\u0443\u0447\u0448\u0435",
      "\u0441\u0432\u043E\u044E",
      "\u0431\u0435\u0437\u043E\u043F\u0430\u0441\u043D\u0443\u044E",
      "\u0441\u0442\u0440\u0430\u043D\u0443",
      "\u043F\u043E\u0441\u043B\u0443\u0448\u0430\u0432\u0448\u0438\u0441\u044C",
      "\u0440\u0443\u043A\u043E\u043F\u0438\u0441\u0438",
      "\u043D\u0430\u0448",
      "\u043F\u0440\u043E\u0434\u043E\u043B\u0436\u0438\u043B",
      "\u0441\u0432\u043E\u0439",
      "\u043F\u0443\u0442\u044C",
      "\u0432\u0441\u043A\u043E\u0440\u0435",
      "\u0435\u043C\u0443",
      "\u043F\u043E\u0432\u0441\u0442\u0440\u0435\u0447\u0430\u043B\u0441\u044F",
      "\u043A\u043E\u0432\u0430\u0440\u043D\u044B\u0439",
      "\u0441\u043E\u0441\u0442\u0430\u0432\u0438\u0442\u0435\u043B\u044C",
      "\u0440\u0435\u043A\u043B\u0430\u043C\u043D\u044B\u0445",
      "\u0442\u0435\u043A\u0441\u0442\u043E\u0432",
      "\u043D\u0430\u043F\u043E\u0438\u0432\u0448\u0438\u0439",
      "\u044F\u0437\u044B\u043A\u043E\u043C",
      "\u0440\u0435\u0447\u044C\u044E",
      "\u0437\u0430\u043C\u0430\u043D\u0438\u0432\u0448\u0438\u0439",
      "\u0441\u0432\u043E\u0435",
      "\u0430\u0433\u0435\u043D\u0442\u0441\u0442\u0432\u043E",
      "\u043A\u043E\u0442\u043E\u0440\u043E\u0435",
      "\u0438\u0441\u043F\u043E\u043B\u044C\u0437\u043E\u0432\u0430\u043B\u043E",
      "\u0441\u043D\u043E\u0432\u0430",
      "\u0441\u043D\u043E\u0432\u0430",
      "\u0441\u0432\u043E\u0438\u0445",
      "\u043F\u0440\u043E\u0435\u043A\u0442\u0430\u0445",
      "\u0435\u0441\u043B\u0438",
      "\u043F\u0435\u0440\u0435\u043F\u0438\u0441\u0430\u043B\u0438",
      "\u0442\u043E",
      "\u0436\u0438\u0432\u0435\u0442",
      "\u0442\u0430\u043C",
      "\u0434\u043E",
      "\u0441\u0438\u0445",
      "\u043F\u043E\u0440"
    ]
  };
  var sp = {
    "common": ["mujer", "uno", "dolor", "m\xE1s", "de", "poder", "mismo", "si"],
    "words": [
      "ejercicio",
      "preferencia",
      "perspicacia",
      "laboral",
      "pa\xF1o",
      "suntuoso",
      "molde",
      "namibia",
      "planeador",
      "mirar",
      "dem\xE1s",
      "oficinista",
      "excepci\xF3n",
      "odio",
      "consecuencia",
      "casi",
      "auto",
      "chicharra",
      "velo",
      "elixir",
      "ataque",
      "no",
      "odio",
      "temporal",
      "cu\xF3rum",
      "dign\xEDsimo",
      "facilismo",
      "letra",
      "nihilista",
      "expedici\xF3n",
      "alma",
      "alveolar",
      "aparte",
      "le\xF3n",
      "animal",
      "como",
      "paria",
      "belleza",
      "modo",
      "natividad",
      "justo",
      "ataque",
      "s\xE9quito",
      "pillo",
      "sed",
      "ex",
      "y",
      "voluminoso",
      "temporalidad",
      "verdades",
      "racional",
      "asunci\xF3n",
      "incidente",
      "marejada",
      "placenta",
      "amanecer",
      "fuga",
      "previsor",
      "presentaci\xF3n",
      "lejos",
      "necesariamente",
      "sospechoso",
      "adiposidad",
      "quind\xEDo",
      "p\xF3cima",
      "voluble",
      "d\xE9bito",
      "sinti\xF3",
      "accesorio",
      "falda",
      "sapiencia",
      "volutas",
      "queso",
      "permacultura",
      "laudo",
      "soluciones",
      "entero",
      "pan",
      "litro",
      "tonelada",
      "culpa",
      "libertario",
      "mosca",
      "dictado",
      "reincidente",
      "nascimiento",
      "dolor",
      "escolar",
      "impedimento",
      "m\xEDnima",
      "mayores",
      "repugnante",
      "dulce",
      "obcecado",
      "monta\xF1a",
      "enigma",
      "total",
      "delet\xE9reo",
      "d\xE9cima",
      "c\xE1bala",
      "fotograf\xEDa",
      "dolores",
      "molesto",
      "olvido",
      "paciencia",
      "resiliencia",
      "voluntad",
      "molestias",
      "magn\xEDfico",
      "distinci\xF3n",
      "ovni",
      "marejada",
      "cerro",
      "torre",
      "y",
      "abogada",
      "manantial",
      "corporal",
      "agua",
      "crep\xFAsculo",
      "ataque",
      "desierto",
      "laboriosamente",
      "angustia",
      "afortunado",
      "alma",
      "encefalograma",
      "materialidad",
      "cosas",
      "o",
      "renuncia",
      "error",
      "menos",
      "conejo",
      "abad\xEDa",
      "analfabeto",
      "remo",
      "fugacidad",
      "oficio",
      "en",
      "alm\xE1cigo",
      "vos",
      "pan",
      "represi\xF3n",
      "n\xFAmeros",
      "triste",
      "refugiado",
      "trote",
      "inventor",
      "corchea",
      "repelente",
      "magma",
      "recusado",
      "patr\xF3n",
      "expl\xEDcito",
      "paloma",
      "s\xEDndrome",
      "inmune",
      "autoinmune",
      "comodidad",
      "ley",
      "vietnamita",
      "demonio",
      "tasmania",
      "repeler",
      "ap\xE9ndice",
      "arquitecto",
      "columna",
      "yugo",
      "computador",
      "mula",
      "a",
      "prop\xF3sito",
      "fantas\xEDa",
      "alias",
      "rayo",
      "tenedor",
      "deleznable",
      "ventana",
      "cara",
      "anemia",
      "corrupto"
    ]
  };
  var vocabularies = { ru, sp, latin };
  var reLorem = /^lorem([a-z]*)(\d*)(-\d*)?$/i;
  function lorem(node, ancestors, config) {
    let m;
    if (node.name && (m = node.name.match(reLorem))) {
      const db = vocabularies[m[1]] || vocabularies.latin;
      const minWordCount = m[2] ? Math.max(1, Number(m[2])) : 30;
      const maxWordCount = m[3] ? Math.max(minWordCount, Number(m[3].slice(1))) : minWordCount;
      const wordCount = rand(minWordCount, maxWordCount);
      const repeat = node.repeat || findRepeater(ancestors);
      node.name = node.attributes = void 0;
      node.value = [paragraph(db, wordCount, !repeat || repeat.value === 0)];
      if (node.repeat && ancestors.length > 1) {
        resolveImplicitTag(node, ancestors, config);
      }
    }
  }
  function rand(from, to) {
    return Math.floor(Math.random() * (to - from) + from);
  }
  function sample(arr, count) {
    const len = arr.length;
    const iterations = Math.min(len, count);
    const result = [];
    while (result.length < iterations) {
      const str = arr[rand(0, len)];
      if (!result.includes(str)) {
        result.push(str);
      }
    }
    return result;
  }
  function choice(val) {
    return val[rand(0, val.length - 1)];
  }
  function sentence(words, end) {
    if (words.length) {
      words = [capitalize(words[0])].concat(words.slice(1));
    }
    return words.join(" ") + (end || choice("?!..."));
  }
  function capitalize(word) {
    return word[0].toUpperCase() + word.slice(1);
  }
  function insertCommas(words) {
    if (words.length < 2) {
      return words;
    }
    words = words.slice();
    const len = words.length;
    const hasComma = /,$/;
    let totalCommas = 0;
    if (len > 3 && len <= 6) {
      totalCommas = rand(0, 1);
    } else if (len > 6 && len <= 12) {
      totalCommas = rand(0, 2);
    } else {
      totalCommas = rand(1, 4);
    }
    for (let i = 0, pos; i < totalCommas; i++) {
      pos = rand(0, len - 2);
      if (!hasComma.test(words[pos])) {
        words[pos] += ",";
      }
    }
    return words;
  }
  function paragraph(dict, wordCount, startWithCommon) {
    const result = [];
    let totalWords = 0;
    let words;
    if (startWithCommon && dict.common) {
      words = dict.common.slice(0, wordCount);
      totalWords += words.length;
      result.push(sentence(insertCommas(words), "."));
    }
    while (totalWords < wordCount) {
      words = sample(dict.words, Math.min(rand(2, 30), wordCount - totalWords));
      totalWords += words.length;
      result.push(sentence(insertCommas(words)));
    }
    return result.join(" ");
  }
  function findRepeater(ancestors) {
    for (let i = ancestors.length - 1; i >= 0; i--) {
      const element3 = ancestors[i];
      if (element3.type === "AbbreviationNode" && element3.repeat) {
        return element3.repeat;
      }
    }
  }
  function jsx(node) {
    if (node.attributes) {
      node.attributes.forEach(rename);
    }
  }
  function rename(attr) {
    if (attr.name === "class") {
      attr.name = "className";
    } else if (attr.name === "for") {
      attr.name = "htmlFor";
    }
  }
  function xsl(node) {
    if (matchesName(node.name) && node.attributes && (node.children.length || node.value)) {
      node.attributes = node.attributes.filter(isAllowed);
    }
  }
  function isAllowed(attr) {
    return attr.name !== "select";
  }
  function matchesName(name) {
    return name === "xsl:variable" || name === "xsl:with-param";
  }
  var reElement = /^(-+)([a-z0-9]+[a-z0-9-]*)/i;
  var reModifier = /^(_+)([a-z0-9]+[a-z0-9-_]*)/i;
  var blockCandidates1 = (className) => /^[a-z]\-/i.test(className);
  var blockCandidates2 = (className) => /^[a-z]/i.test(className);
  function bem(node, ancestors, config) {
    expandClassNames(node);
    expandShortNotation(node, ancestors, config);
  }
  function expandClassNames(node) {
    const data = getBEMData(node);
    const classNames = [];
    for (const cl of data.classNames) {
      const ix = cl.indexOf("_");
      if (ix > 0 && !cl.startsWith("-")) {
        classNames.push(cl.slice(0, ix));
        classNames.push(cl.slice(ix));
      } else {
        classNames.push(cl);
      }
    }
    if (classNames.length) {
      data.classNames = classNames.filter(uniqueClass);
      data.block = findBlockName(data.classNames);
      updateClass(node, data.classNames.join(" "));
    }
  }
  function expandShortNotation(node, ancestors, config) {
    const data = getBEMData(node);
    const classNames = [];
    const { options } = config;
    const path = ancestors.slice(1).concat(node);
    for (let cl of data.classNames) {
      let prefix = "";
      let m;
      const originalClass = cl;
      if (m = cl.match(reElement)) {
        prefix = getBlockName(path, m[1].length, config.context) + options["bem.element"] + m[2];
        classNames.push(prefix);
        cl = cl.slice(m[0].length);
      }
      if (m = cl.match(reModifier)) {
        if (!prefix) {
          prefix = getBlockName(path, m[1].length);
          classNames.push(prefix);
        }
        classNames.push(`${prefix}${options["bem.modifier"]}${m[2]}`);
        cl = cl.slice(m[0].length);
      }
      if (cl === originalClass) {
        classNames.push(originalClass);
      }
    }
    const arrClassNames = classNames.filter(uniqueClass);
    if (arrClassNames.length) {
      updateClass(node, arrClassNames.join(" "));
    }
  }
  function getBEMData(node) {
    if (!node._bem) {
      let classValue = "";
      if (node.attributes) {
        for (const attr of node.attributes) {
          if (attr.name === "class" && attr.value) {
            classValue = stringifyValue2(attr.value);
            break;
          }
        }
      }
      node._bem = parseBEM(classValue);
    }
    return node._bem;
  }
  function getBEMDataFromContext(context) {
    if (!context._bem) {
      context._bem = parseBEM(context.attributes && context.attributes.class || "");
    }
    return context._bem;
  }
  function parseBEM(classValue) {
    const classNames = classValue ? classValue.split(/\s+/) : [];
    return {
      classNames,
      block: findBlockName(classNames)
    };
  }
  function getBlockName(ancestors, depth = 0, context) {
    const maxParentIx = 0;
    let parentIx = Math.max(ancestors.length - depth, maxParentIx);
    do {
      const parent = ancestors[parentIx];
      if (parent) {
        const data = getBEMData(parent);
        if (data.block) {
          return data.block;
        }
      }
    } while (maxParentIx < parentIx--);
    if (context) {
      const data = getBEMDataFromContext(context);
      if (data.block) {
        return data.block;
      }
    }
    return "";
  }
  function findBlockName(classNames) {
    return find(classNames, blockCandidates1) || find(classNames, blockCandidates2) || void 0;
  }
  function find(classNames, filter) {
    for (const cl of classNames) {
      if (reElement.test(cl) || reModifier.test(cl)) {
        break;
      }
      if (filter(cl)) {
        return cl;
      }
    }
  }
  function updateClass(node, value) {
    for (const attr of node.attributes) {
      if (attr.name === "class") {
        attr.value = [value];
        break;
      }
    }
  }
  function stringifyValue2(value) {
    let result = "";
    for (const t of value) {
      result += typeof t === "string" ? t : t.name;
    }
    return result;
  }
  function uniqueClass(item, ix, arr) {
    return !!item && arr.indexOf(item) === ix;
  }
  function walk$1(abbr, visitor, state) {
    const callback = (ctx, index, items) => {
      const { parent, current } = state;
      state.parent = current;
      state.current = ctx;
      visitor(ctx, index, items, state, next2);
      state.current = current;
      state.parent = parent;
    };
    const next2 = (node, index, items) => {
      state.ancestors.push(state.current);
      callback(node, index, items);
      state.ancestors.pop();
    };
    abbr.children.forEach(callback);
  }
  function createWalkState(config) {
    return {
      current: null,
      parent: void 0,
      ancestors: [],
      config,
      field: 1,
      out: createOutputStream(config.options)
    };
  }
  var caret = [{ type: "Field", index: 0, name: "" }];
  function isSnippet(node) {
    return node ? !node.name && !node.attributes : false;
  }
  function isInlineElement(node, config) {
    return node ? isInline(node, config) : false;
  }
  function isField2(token) {
    return typeof token === "object" && token.type === "Field";
  }
  function pushTokens(tokens, state) {
    const { out } = state;
    let largestIndex = -1;
    for (const t of tokens) {
      if (typeof t === "string") {
        pushString(out, t);
      } else {
        pushField(out, state.field + t.index, t.name);
        if (t.index > largestIndex) {
          largestIndex = t.index;
        }
      }
    }
    if (largestIndex !== -1) {
      state.field += largestIndex + 1;
    }
  }
  function splitByLines$1(tokens) {
    const result = [];
    let line = [];
    for (const t of tokens) {
      if (typeof t === "string") {
        const lines = t.split(/\r\n?|\n/g);
        line.push(lines.shift() || "");
        while (lines.length) {
          result.push(line);
          line = [lines.shift() || ""];
        }
      } else {
        line.push(t);
      }
    }
    line.length && result.push(line);
    return result;
  }
  function shouldOutputAttribute(attr) {
    return !attr.implied || attr.valueType !== "raw" || !!attr.value && attr.value.length > 0;
  }
  function template(text2) {
    const tokens = [];
    const scanner = { pos: 0, text: text2 };
    let placeholder;
    let offset = scanner.pos;
    let pos = scanner.pos;
    while (scanner.pos < scanner.text.length) {
      pos = scanner.pos;
      if (placeholder = consumePlaceholder3(scanner)) {
        if (offset !== scanner.pos) {
          tokens.push(text2.slice(offset, pos));
        }
        tokens.push(placeholder);
        offset = scanner.pos;
      } else {
        scanner.pos++;
      }
    }
    if (offset !== scanner.pos) {
      tokens.push(text2.slice(offset));
    }
    return tokens;
  }
  function consumePlaceholder3(scanner) {
    if (peek3(scanner) === 91) {
      const start = ++scanner.pos;
      let namePos = start;
      let afterPos = start;
      let stack = 1;
      while (scanner.pos < scanner.text.length) {
        const code2 = peek3(scanner);
        if (isTokenStart(code2)) {
          namePos = scanner.pos;
          while (isToken(peek3(scanner))) {
            scanner.pos++;
          }
          afterPos = scanner.pos;
        } else {
          if (code2 === 91) {
            stack++;
          } else if (code2 === 93) {
            if (--stack === 0) {
              return {
                before: scanner.text.slice(start, namePos),
                after: scanner.text.slice(afterPos, scanner.pos++),
                name: scanner.text.slice(namePos, afterPos)
              };
            }
          }
          scanner.pos++;
        }
      }
    }
  }
  function peek3(scanner, pos = scanner.pos) {
    return scanner.text.charCodeAt(pos);
  }
  function isTokenStart(code2) {
    return code2 >= 65 && code2 <= 90;
  }
  function isToken(code2) {
    return isTokenStart(code2) || code2 > 47 && code2 < 58 || code2 === 95 || code2 === 45;
  }
  function createCommentState(config) {
    const { options } = config;
    return {
      enabled: options["comment.enabled"],
      trigger: options["comment.trigger"],
      before: options["comment.before"] ? template(options["comment.before"]) : void 0,
      after: options["comment.after"] ? template(options["comment.after"]) : void 0
    };
  }
  function commentNodeBefore(node, state) {
    if (shouldComment(node, state) && state.comment.before) {
      output(node, state.comment.before, state);
    }
  }
  function commentNodeAfter(node, state) {
    if (shouldComment(node, state) && state.comment.after) {
      output(node, state.comment.after, state);
    }
  }
  function shouldComment(node, state) {
    const { comment } = state;
    if (!comment.enabled || !comment.trigger || !node.name || !node.attributes) {
      return false;
    }
    for (const attr of node.attributes) {
      if (attr.name && comment.trigger.includes(attr.name)) {
        return true;
      }
    }
    return false;
  }
  function output(node, tokens, state) {
    const attrs = {};
    const { out } = state;
    for (const attr of node.attributes) {
      if (attr.name && attr.value) {
        attrs[attr.name.toUpperCase()] = attr.value;
      }
    }
    for (const token of tokens) {
      if (typeof token === "string") {
        pushString(out, token);
      } else if (attrs[token.name]) {
        pushString(out, token.before);
        pushTokens(attrs[token.name], state);
        pushString(out, token.after);
      }
    }
  }
  var htmlTagRegex = /^<([\w\-:]+)[\s>]/;
  function html(abbr, config) {
    const state = createWalkState(config);
    state.comment = createCommentState(config);
    walk$1(abbr, element2, state);
    return state.out.value;
  }
  function element2(node, index, items, state, next2) {
    const { out, config } = state;
    const format = shouldFormat(node, index, items, state);
    const level = getIndent(state);
    out.level += level;
    format && pushNewline(out, true);
    if (node.name) {
      const name = tagName(node.name, config);
      commentNodeBefore(node, state);
      pushString(out, `<${name}`);
      if (node.attributes) {
        for (const attr of node.attributes) {
          if (shouldOutputAttribute(attr)) {
            pushAttribute(attr, state);
          }
        }
      }
      if (node.selfClosing && !node.children.length && !node.value) {
        pushString(out, `${selfClose(config)}>`);
      } else {
        pushString(out, ">");
        if (!pushSnippet(node, state, next2)) {
          if (node.value) {
            const innerFormat = node.value.some(hasNewline) || startsWithBlockTag(node.value, config);
            innerFormat && pushNewline(state.out, ++out.level);
            pushTokens(node.value, state);
            innerFormat && pushNewline(state.out, --out.level);
          }
          node.children.forEach(next2);
          if (!node.value && !node.children.length) {
            const innerFormat = config.options["output.formatLeafNode"] || config.options["output.formatForce"].includes(node.name);
            innerFormat && pushNewline(state.out, ++out.level);
            pushTokens(caret, state);
            innerFormat && pushNewline(state.out, --out.level);
          }
        }
        pushString(out, `</${name}>`);
        commentNodeAfter(node, state);
      }
    } else if (!pushSnippet(node, state, next2) && node.value) {
      pushTokens(node.value, state);
      node.children.forEach(next2);
    }
    if (format && index === items.length - 1 && state.parent) {
      const offset = isSnippet(state.parent) ? 0 : 1;
      pushNewline(out, out.level - offset);
    }
    out.level -= level;
  }
  function pushAttribute(attr, state) {
    const { out, config } = state;
    if (attr.name) {
      const name = attrName(attr.name, config);
      const lQuote = attrQuote(attr, config, true);
      const rQuote = attrQuote(attr, config);
      let value = attr.value;
      if (isBooleanAttribute(attr, config) && !value) {
        if (!config.options["output.compactBoolean"]) {
          value = [name];
        }
      } else if (!value) {
        value = caret;
      }
      pushString(out, " " + name);
      if (value) {
        pushString(out, "=" + lQuote);
        pushTokens(value, state);
        pushString(out, rQuote);
      } else if (config.options["output.selfClosingStyle"] !== "html") {
        pushString(out, "=" + lQuote + rQuote);
      }
    }
  }
  function pushSnippet(node, state, next2) {
    if (node.value && node.children.length) {
      const fieldIx = node.value.findIndex(isField2);
      if (fieldIx !== -1) {
        pushTokens(node.value.slice(0, fieldIx), state);
        const line = state.out.line;
        let pos = fieldIx + 1;
        node.children.forEach(next2);
        if (state.out.line !== line && typeof node.value[pos] === "string") {
          pushString(state.out, node.value[pos++].trimLeft());
        }
        pushTokens(node.value.slice(pos), state);
        return true;
      }
    }
    return false;
  }
  function shouldFormat(node, index, items, state) {
    const { config, parent } = state;
    if (!config.options["output.format"]) {
      return false;
    }
    if (index === 0 && !parent) {
      return false;
    }
    if (parent && isSnippet(parent) && items.length === 1) {
      return false;
    }
    if (isSnippet(node)) {
      const format = isSnippet(items[index - 1]) || isSnippet(items[index + 1]) || node.value.some(hasNewline) || node.value.some(isField2) && node.children.length;
      if (format) {
        return true;
      }
    }
    if (isInline(node, config)) {
      if (index === 0) {
        for (let i = 0; i < items.length; i++) {
          if (!isInline(items[i], config)) {
            return true;
          }
        }
      } else if (!isInline(items[index - 1], config)) {
        return true;
      }
      if (config.options["output.inlineBreak"]) {
        let adjacentInline = 1;
        let before = index;
        let after = index;
        while (isInlineElement(items[--before], config)) {
          adjacentInline++;
        }
        while (isInlineElement(items[++after], config)) {
          adjacentInline++;
        }
        if (adjacentInline >= config.options["output.inlineBreak"]) {
          return true;
        }
      }
      for (let i = 0, il = node.children.length; i < il; i++) {
        if (shouldFormat(node.children[i], i, node.children, state)) {
          return true;
        }
      }
      return false;
    }
    return true;
  }
  function getIndent(state) {
    const { config, parent } = state;
    if (!parent || isSnippet(parent) || parent.name && config.options["output.formatSkip"].includes(parent.name)) {
      return 0;
    }
    return 1;
  }
  function hasNewline(value) {
    return typeof value === "string" && /\r|\n/.test(value);
  }
  function startsWithBlockTag(value, config) {
    if (value.length && typeof value[0] === "string") {
      const matches = htmlTagRegex.exec(value[0]);
      if ((matches === null || matches === void 0 ? void 0 : matches.length) && !config.options["inlineElements"].includes(matches[1].toLowerCase())) {
        return true;
      }
    }
    return false;
  }
  function indentFormat(abbr, config, options) {
    const state = createWalkState(config);
    state.options = options || {};
    walk$1(abbr, element$1, state);
    return state.out.value;
  }
  function element$1(node, index, items, state, next2) {
    const { out, options } = state;
    const { primary, secondary } = collectAttributes(node);
    const level = state.parent ? 1 : 0;
    out.level += level;
    if (shouldFormat$1(node, index, items, state)) {
      pushNewline(out, true);
    }
    if (node.name && (node.name !== "div" || !primary.length)) {
      pushString(out, (options.beforeName || "") + node.name + (options.afterName || ""));
    }
    pushPrimaryAttributes(primary, state);
    pushSecondaryAttributes(secondary.filter(shouldOutputAttribute), state);
    if (node.selfClosing && !node.value && !node.children.length) {
      if (state.options.selfClose) {
        pushString(out, state.options.selfClose);
      }
    } else {
      pushValue(node, state);
      node.children.forEach(next2);
    }
    out.level -= level;
  }
  function collectAttributes(node) {
    const primary = [];
    const secondary = [];
    if (node.attributes) {
      for (const attr of node.attributes) {
        if (isPrimaryAttribute(attr)) {
          primary.push(attr);
        } else {
          secondary.push(attr);
        }
      }
    }
    return { primary, secondary };
  }
  function pushPrimaryAttributes(attrs, state) {
    for (const attr of attrs) {
      if (attr.value) {
        if (attr.name === "class") {
          pushString(state.out, ".");
          const tokens = attr.value.map((t) => typeof t === "string" ? t.replace(/\s+/g, ".") : t);
          pushTokens(tokens, state);
        } else {
          pushString(state.out, "#");
          pushTokens(attr.value, state);
        }
      }
    }
  }
  function pushSecondaryAttributes(attrs, state) {
    if (attrs.length) {
      const { out, config, options } = state;
      options.beforeAttribute && pushString(out, options.beforeAttribute);
      for (let i = 0; i < attrs.length; i++) {
        const attr = attrs[i];
        pushString(out, attrName(attr.name || "", config));
        if (isBooleanAttribute(attr, config) && !attr.value) {
          if (!config.options["output.compactBoolean"] && options.booleanValue) {
            pushString(out, "=" + options.booleanValue);
          }
        } else {
          pushString(out, "=" + attrQuote(attr, config, true));
          pushTokens(attr.value || caret, state);
          pushString(out, attrQuote(attr, config));
        }
        if (i !== attrs.length - 1 && options.glueAttribute) {
          pushString(out, options.glueAttribute);
        }
      }
      options.afterAttribute && pushString(out, options.afterAttribute);
    }
  }
  function pushValue(node, state) {
    if (!node.value && node.children.length) {
      return;
    }
    const value = node.value || caret;
    const lines = splitByLines$1(value);
    const { out, options } = state;
    if (lines.length === 1) {
      if (node.name || node.attributes) {
        push(out, " ");
      }
      pushTokens(value, state);
    } else {
      const lineLengths = [];
      let maxLength = 0;
      for (const line of lines) {
        const len = valueLength(line);
        lineLengths.push(len);
        if (len > maxLength) {
          maxLength = len;
        }
      }
      out.level++;
      for (let i = 0; i < lines.length; i++) {
        pushNewline(out, true);
        options.beforeTextLine && push(out, options.beforeTextLine);
        pushTokens(lines[i], state);
        if (options.afterTextLine) {
          push(out, " ".repeat(maxLength - lineLengths[i]));
          push(out, options.afterTextLine);
        }
      }
      out.level--;
    }
  }
  function isPrimaryAttribute(attr) {
    return attr.name === "class" || attr.name === "id";
  }
  function valueLength(tokens) {
    let len = 0;
    for (const token of tokens) {
      len += typeof token === "string" ? token.length : token.name.length;
    }
    return len;
  }
  function shouldFormat$1(node, index, items, state) {
    if (!state.parent && index === 0) {
      return false;
    }
    return !isSnippet(node);
  }
  function haml(abbr, config) {
    return indentFormat(abbr, config, {
      beforeName: "%",
      beforeAttribute: "(",
      afterAttribute: ")",
      glueAttribute: " ",
      afterTextLine: " |",
      booleanValue: "true",
      selfClose: "/"
    });
  }
  function slim(abbr, config) {
    return indentFormat(abbr, config, {
      beforeAttribute: " ",
      glueAttribute: " ",
      beforeTextLine: "| ",
      selfClose: "/"
    });
  }
  function pug(abbr, config) {
    return indentFormat(abbr, config, {
      beforeAttribute: "(",
      afterAttribute: ")",
      glueAttribute: ", ",
      beforeTextLine: "| ",
      selfClose: config.options["output.selfClosingStyle"] === "xml" ? "/" : ""
    });
  }
  var formatters = { html, haml, slim, pug };
  function parse2(abbr, config) {
    let oldTextValue;
    if (typeof abbr === "string") {
      let parseOpt = config;
      if (config.options["jsx.enabled"]) {
        parseOpt = Object.assign(Object.assign({}, parseOpt), { jsx: true });
      }
      if (config.options["markup.href"]) {
        parseOpt = Object.assign(Object.assign({}, parseOpt), { href: true });
      }
      abbr = abbreviation_es_default(abbr, parseOpt);
      oldTextValue = config.text;
      config.text = void 0;
    }
    abbr = resolveSnippets(abbr, config);
    walk(abbr, transform, config);
    config.text = oldTextValue !== null && oldTextValue !== void 0 ? oldTextValue : config.text;
    return abbr;
  }
  function stringify2(abbr, config) {
    const formatter = formatters[config.syntax] || html;
    return formatter(abbr, config);
  }
  function transform(node, ancestors, config) {
    implicitTag(node, ancestors, config);
    mergeAttributes(node, config);
    lorem(node, ancestors, config);
    if (config.syntax === "xsl") {
      xsl(node);
    }
    if (config.options["jsx.enabled"]) {
      jsx(node);
    }
    if (config.options["bem.enabled"]) {
      bem(node, ancestors, config);
    }
  }
  var reProperty = /^([a-z-]+)(?:\s*:\s*([^\n\r;]+?);*)?$/;
  var opt = { value: true };
  function createSnippet(key, value) {
    const m = value.match(reProperty);
    if (m) {
      const keywords = {};
      const parsed = m[2] ? m[2].split("|").map(parseValue) : [];
      for (const item of parsed) {
        for (const cssVal of item) {
          collectKeywords(cssVal, keywords);
        }
      }
      return {
        type: "Property",
        key,
        property: m[1],
        value: parsed,
        keywords,
        dependencies: []
      };
    }
    return { type: "Raw", key, value };
  }
  function nest(snippets) {
    snippets = snippets.slice().sort(snippetsSort);
    const stack = [];
    let prev;
    for (const cur of snippets.filter(isProperty)) {
      while (stack.length) {
        prev = stack[stack.length - 1];
        if (cur.property.startsWith(prev.property) && cur.property.charCodeAt(prev.property.length) === 45) {
          prev.dependencies.push(cur);
          stack.push(cur);
          break;
        }
        stack.pop();
      }
      if (!stack.length) {
        stack.push(cur);
      }
    }
    return snippets;
  }
  function snippetsSort(a, b) {
    if (a.key === b.key) {
      return 0;
    }
    return a.key < b.key ? -1 : 1;
  }
  function parseValue(value) {
    return css_abbreviation_es_default(value.trim(), opt)[0].value;
  }
  function isProperty(snippet) {
    return snippet.type === "Property";
  }
  function collectKeywords(cssVal, dest) {
    for (const v of cssVal.value) {
      if (v.type === "Literal") {
        dest[v.value] = v;
      } else if (v.type === "FunctionCall") {
        dest[v.name] = v;
      } else if (v.type === "Field") {
        const value = v.name.trim();
        if (value) {
          dest[value] = { type: "Literal", value };
        }
      }
    }
  }
  function scoreMatch(str1, str2, partialMatch = false) {
    str1 = str1.toLowerCase();
    str2 = str2.toLowerCase();
    if (str1 === str2) {
      return 1;
    }
    if (!str1 || !str2 || str1.charCodeAt(0) !== str2.charCodeAt(0)) {
      return 0;
    }
    const str1Len = str1.length;
    const str2Len = str2.length;
    if (!partialMatch && str1Len > str2Len) {
      return 0;
    }
    const minLength = Math.min(str1Len, str2Len);
    const maxLength = Math.max(str1Len, str2Len);
    let i = 1;
    let j = 1;
    let score = maxLength;
    let ch1 = 0;
    let ch2 = 0;
    let found = false;
    let acronym = false;
    while (i < str1Len) {
      ch1 = str1.charCodeAt(i);
      found = false;
      acronym = false;
      while (j < str2Len) {
        ch2 = str2.charCodeAt(j);
        if (ch1 === ch2) {
          found = true;
          score += maxLength - (acronym ? i : j);
          break;
        }
        acronym = ch2 === 45;
        j++;
      }
      if (!found) {
        if (!partialMatch) {
          return 0;
        }
        break;
      }
      i++;
    }
    const matchRatio = i / maxLength;
    const delta = maxLength - minLength;
    const maxScore = sum(maxLength) - sum(delta);
    return score * matchRatio / maxScore;
  }
  function sum(n) {
    return n * (n + 1) / 2;
  }
  function color(token, shortHex) {
    if (!token.r && !token.g && !token.b && !token.a) {
      return "transparent";
    } else if (token.a === 1) {
      return asHex(token, shortHex);
    }
    return asRGB(token);
  }
  function asHex(token, short) {
    const fn = short && isShortHex(token.r) && isShortHex(token.g) && isShortHex(token.b) ? toShortHex : toHex;
    return "#" + fn(token.r) + fn(token.g) + fn(token.b);
  }
  function asRGB(token) {
    const values = [token.r, token.g, token.b];
    if (token.a !== 1) {
      values.push(frac(token.a, 8));
    }
    return `${values.length === 3 ? "rgb" : "rgba"}(${values.join(", ")})`;
  }
  function frac(num, digits = 4) {
    return num.toFixed(digits).replace(/\.?0+$/, "");
  }
  function isShortHex(hex) {
    return !(hex % 17);
  }
  function toShortHex(num) {
    return (num >> 4).toString(16);
  }
  function toHex(num) {
    return pad(num.toString(16), 2);
  }
  function pad(value, len) {
    while (value.length < len) {
      value = "0" + value;
    }
    return value;
  }
  function css(abbr, config) {
    var _a;
    const out = createOutputStream(config.options);
    const format = config.options["output.format"];
    if (((_a = config.context) === null || _a === void 0 ? void 0 : _a.name) === "@@section") {
      abbr = abbr.filter((node) => node.snippet);
    }
    for (let i = 0; i < abbr.length; i++) {
      if (format && i !== 0) {
        pushNewline(out, true);
      }
      property(abbr[i], out, config);
    }
    return out.value;
  }
  function property(node, out, config) {
    const isJSON = config.options["stylesheet.json"];
    if (node.name) {
      const name = isJSON ? toCamelCase(node.name) : node.name;
      pushString(out, name + config.options["stylesheet.between"]);
      if (node.value.length) {
        propertyValue(node, out, config);
      } else {
        pushField(out, 0, "");
      }
      if (isJSON) {
        push(out, ",");
      } else {
        outputImportant(node, out, true);
        push(out, config.options["stylesheet.after"]);
      }
    } else {
      for (const cssVal of node.value) {
        for (const v of cssVal.value) {
          outputToken(v, out, config);
        }
      }
      outputImportant(node, out, node.value.length > 0);
    }
  }
  function propertyValue(node, out, config) {
    const isJSON = config.options["stylesheet.json"];
    const num = isJSON ? getSingleNumeric(node) : null;
    if (num && (!num.unit || num.unit === "px")) {
      push(out, String(num.value));
    } else {
      const quote2 = getQuote(config);
      isJSON && push(out, quote2);
      for (let i = 0; i < node.value.length; i++) {
        if (i !== 0) {
          push(out, ", ");
        }
        outputValue(node.value[i], out, config);
      }
      isJSON && push(out, quote2);
    }
  }
  function outputImportant(node, out, separator) {
    if (node.important) {
      if (separator) {
        push(out, " ");
      }
      push(out, "!important");
    }
  }
  function outputValue(value, out, config) {
    for (let i = 0, prevEnd = -1; i < value.value.length; i++) {
      const token = value.value[i];
      if (i !== 0 && (token.type !== "Field" || token.start !== prevEnd)) {
        push(out, " ");
      }
      outputToken(token, out, config);
      prevEnd = token["end"];
    }
  }
  function outputToken(token, out, config) {
    if (token.type === "ColorValue") {
      push(out, color(token, config.options["stylesheet.shortHex"]));
    } else if (token.type === "Literal") {
      pushString(out, token.value);
    } else if (token.type === "NumberValue") {
      pushString(out, frac(token.value, 4) + token.unit);
    } else if (token.type === "StringValue") {
      const quote2 = token.quote === "double" ? '"' : "'";
      pushString(out, quote2 + token.value + quote2);
    } else if (token.type === "Field") {
      pushField(out, token.index, token.name);
    } else if (token.type === "FunctionCall") {
      push(out, token.name + "(");
      for (let i = 0; i < token.arguments.length; i++) {
        if (i) {
          push(out, ", ");
        }
        outputValue(token.arguments[i], out, config);
      }
      push(out, ")");
    }
  }
  function getSingleNumeric(node) {
    if (node.value.length === 1) {
      const cssVal = node.value[0];
      if (cssVal.value.length === 1 && cssVal.value[0].type === "NumberValue") {
        return cssVal.value[0];
      }
    }
  }
  function toCamelCase(str) {
    return str.replace(/\-(\w)/g, (_, letter) => letter.toUpperCase());
  }
  function getQuote(config) {
    return config.options["stylesheet.jsonDoubleQuotes"] ? '"' : "'";
  }
  var gradientName = "lg";
  function parse$1(abbr, config) {
    var _a;
    const snippets = ((_a = config.cache) === null || _a === void 0 ? void 0 : _a.stylesheetSnippets) || convertSnippets(config.snippets);
    if (config.cache) {
      config.cache.stylesheetSnippets = snippets;
    }
    if (typeof abbr === "string") {
      abbr = css_abbreviation_es_default(abbr, { value: isValueScope(config) });
    }
    const filteredSnippets = getSnippetsForScope(snippets, config);
    for (const node of abbr) {
      resolveNode(node, filteredSnippets, config);
    }
    return abbr;
  }
  function convertSnippets(snippets) {
    const result = [];
    for (const key of Object.keys(snippets)) {
      result.push(createSnippet(key, snippets[key]));
    }
    return nest(result);
  }
  function resolveNode(node, snippets, config) {
    if (!resolveGradient(node, config)) {
      const score = config.options["stylesheet.fuzzySearchMinScore"];
      if (isValueScope(config)) {
        const propName = config.context.name;
        const snippet = snippets.find((s) => s.type === "Property" && s.property === propName);
        resolveValueKeywords(node, config, snippet, score);
        node.snippet = snippet;
      } else if (node.name) {
        const snippet = findBestMatch(node.name, snippets, score, true);
        node.snippet = snippet;
        if (snippet) {
          if (snippet.type === "Property") {
            resolveAsProperty(node, snippet, config);
          } else {
            resolveAsSnippet(node, snippet);
          }
        }
      }
    }
    if (node.name || config.context) {
      resolveNumericValue(node, config);
    }
    return node;
  }
  function resolveGradient(node, config) {
    let gradientFn = null;
    const cssVal = node.value.length === 1 ? node.value[0] : null;
    if (cssVal && cssVal.value.length === 1) {
      const v = cssVal.value[0];
      if (v.type === "FunctionCall" && v.name === gradientName) {
        gradientFn = v;
      }
    }
    if (gradientFn || node.name === gradientName) {
      if (!gradientFn) {
        gradientFn = {
          type: "FunctionCall",
          name: "linear-gradient",
          arguments: [cssValue(field3(0, ""))]
        };
      } else {
        gradientFn = Object.assign(Object.assign({}, gradientFn), { name: "linear-gradient" });
      }
      if (!config.context) {
        node.name = "background-image";
      }
      node.value = [cssValue(gradientFn)];
      return true;
    }
    return false;
  }
  function resolveAsProperty(node, snippet, config) {
    const abbr = node.name;
    const inlineValue = getUnmatchedPart(abbr, snippet.key);
    if (inlineValue) {
      if (node.value.length) {
        return node;
      }
      const kw = resolveKeyword(inlineValue, config, snippet);
      if (!kw) {
        return node;
      }
      node.value.push(cssValue(kw));
    }
    node.name = snippet.property;
    if (node.value.length) {
      resolveValueKeywords(node, config, snippet);
    } else if (snippet.value.length) {
      const defaultValue = snippet.value[0];
      node.value = snippet.value.length === 1 || defaultValue.some(hasField) ? defaultValue : defaultValue.map((n) => wrapWithField(n, config));
    }
    return node;
  }
  function resolveValueKeywords(node, config, snippet, minScore) {
    for (const cssVal of node.value) {
      const value = [];
      for (const token of cssVal.value) {
        if (token.type === "Literal") {
          value.push(resolveKeyword(token.value, config, snippet, minScore) || token);
        } else if (token.type === "FunctionCall") {
          const match = resolveKeyword(token.name, config, snippet, minScore);
          if (match && match.type === "FunctionCall") {
            value.push(Object.assign(Object.assign({}, match), { arguments: token.arguments.concat(match.arguments.slice(token.arguments.length)) }));
          } else {
            value.push(token);
          }
        } else {
          value.push(token);
        }
      }
      cssVal.value = value;
    }
  }
  function resolveAsSnippet(node, snippet) {
    let offset = 0;
    let m;
    const reField = /\$\{(\d+)(:[^}]+)?\}/g;
    const inputValue = node.value[0];
    const outputValue2 = [];
    while (m = reField.exec(snippet.value)) {
      if (offset !== m.index) {
        outputValue2.push(literal3(snippet.value.slice(offset, m.index)));
      }
      offset = m.index + m[0].length;
      if (inputValue && inputValue.value.length) {
        outputValue2.push(inputValue.value.shift());
      } else {
        outputValue2.push(field3(Number(m[1]), m[2] ? m[2].slice(1) : ""));
      }
    }
    const tail = snippet.value.slice(offset);
    if (tail) {
      outputValue2.push(literal3(tail));
    }
    node.name = void 0;
    node.value = [cssValue(...outputValue2)];
    return node;
  }
  function findBestMatch(abbr, items, minScore = 0, partialMatch = false) {
    let matchedItem = null;
    let maxScore = 0;
    for (const item of items) {
      const score = scoreMatch(abbr, getScoringPart(item), partialMatch);
      if (score === 1) {
        return item;
      }
      if (score && score >= maxScore) {
        maxScore = score;
        matchedItem = item;
      }
    }
    return maxScore >= minScore ? matchedItem : null;
  }
  function getScoringPart(item) {
    return typeof item === "string" ? item : item.key;
  }
  function getUnmatchedPart(abbr, str) {
    for (let i = 0, lastPos = 0; i < abbr.length; i++) {
      lastPos = str.indexOf(abbr[i], lastPos);
      if (lastPos === -1) {
        return abbr.slice(i);
      }
      lastPos++;
    }
    return "";
  }
  function resolveKeyword(kw, config, snippet, minScore) {
    let ref;
    if (snippet) {
      if (ref = findBestMatch(kw, Object.keys(snippet.keywords), minScore)) {
        return snippet.keywords[ref];
      }
      for (const dep of snippet.dependencies) {
        if (ref = findBestMatch(kw, Object.keys(dep.keywords), minScore)) {
          return dep.keywords[ref];
        }
      }
    }
    if (ref = findBestMatch(kw, config.options["stylesheet.keywords"], minScore)) {
      return literal3(ref);
    }
    return null;
  }
  function resolveNumericValue(node, config) {
    const aliases = config.options["stylesheet.unitAliases"];
    const unitless = config.options["stylesheet.unitless"];
    for (const v of node.value) {
      for (const t of v.value) {
        if (t.type === "NumberValue") {
          if (t.unit) {
            t.unit = aliases[t.unit] || t.unit;
          } else if (t.value !== 0 && !unitless.includes(node.name)) {
            t.unit = t.rawValue.includes(".") ? config.options["stylesheet.floatUnit"] : config.options["stylesheet.intUnit"];
          }
        }
      }
    }
  }
  function cssValue(...args) {
    return {
      type: "CSSValue",
      value: args
    };
  }
  function literal3(value) {
    return { type: "Literal", value };
  }
  function field3(index, name) {
    return { type: "Field", index, name };
  }
  function hasField(value) {
    for (const v of value.value) {
      if (v.type === "Field" || v.type === "FunctionCall" && v.arguments.some(hasField)) {
        return true;
      }
    }
    return false;
  }
  function wrapWithField(node, config, state = { index: 1 }) {
    let value = [];
    for (const v of node.value) {
      switch (v.type) {
        case "ColorValue":
          value.push(field3(state.index++, color(v, config.options["stylesheet.shortHex"])));
          break;
        case "Literal":
          value.push(field3(state.index++, v.value));
          break;
        case "NumberValue":
          value.push(field3(state.index++, `${v.value}${v.unit}`));
          break;
        case "StringValue":
          const q = v.quote === "single" ? "'" : '"';
          value.push(field3(state.index++, q + v.value + q));
          break;
        case "FunctionCall":
          value.push(field3(state.index++, v.name), literal3("("));
          for (let i = 0, il = v.arguments.length; i < il; i++) {
            value = value.concat(wrapWithField(v.arguments[i], config, state).value);
            if (i !== il - 1) {
              value.push(literal3(", "));
            }
          }
          value.push(literal3(")"));
          break;
        default:
          value.push(v);
      }
    }
    return Object.assign(Object.assign({}, node), { value });
  }
  function isValueScope(config) {
    if (config.context) {
      return config.context.name === "@@value" || !config.context.name.startsWith("@@");
    }
    return false;
  }
  function getSnippetsForScope(snippets, config) {
    if (config.context) {
      if (config.context.name === "@@section") {
        return snippets.filter((s) => s.type === "Raw");
      }
      if (config.context.name === "@@property") {
        return snippets.filter((s) => s.type === "Property");
      }
    }
    return snippets;
  }
  var markupSnippets = {
    "a": "a[href]",
    "a:blank": "a[href='http://${0}' target='_blank' rel='noopener noreferrer']",
    "a:link": "a[href='http://${0}']",
    "a:mail": "a[href='mailto:${0}']",
    "a:tel": "a[href='tel:+${0}']",
    "abbr": "abbr[title]",
    "acr|acronym": "acronym[title]",
    "base": "base[href]/",
    "basefont": "basefont/",
    "br": "br/",
    "frame": "frame/",
    "hr": "hr/",
    "bdo": "bdo[dir]",
    "bdo:r": "bdo[dir=rtl]",
    "bdo:l": "bdo[dir=ltr]",
    "col": "col/",
    "link": "link[rel=stylesheet href]/",
    "link:css": "link[href='${1:style}.css']",
    "link:print": "link[href='${1:print}.css' media=print]",
    "link:favicon": "link[rel='shortcut icon' type=image/x-icon href='${1:favicon.ico}']",
    "link:mf|link:manifest": "link[rel='manifest' href='${1:manifest.json}']",
    "link:touch": "link[rel=apple-touch-icon href='${1:favicon.png}']",
    "link:rss": "link[rel=alternate type=application/rss+xml title=RSS href='${1:rss.xml}']",
    "link:atom": "link[rel=alternate type=application/atom+xml title=Atom href='${1:atom.xml}']",
    "link:im|link:import": "link[rel=import href='${1:component}.html']",
    "meta": "meta/",
    "meta:utf": "meta[http-equiv=Content-Type content='text/html;charset=UTF-8']",
    "meta:vp": "meta[name=viewport content='width=${1:device-width}, initial-scale=${2:1.0}']",
    "meta:compat": "meta[http-equiv=X-UA-Compatible content='${1:IE=7}']",
    "meta:edge": "meta:compat[content='${1:ie=edge}']",
    "meta:redirect": "meta[http-equiv=refresh content='0; url=${1:http://example.com}']",
    "meta:kw": "meta[name=keywords content]",
    "meta:desc": "meta[name=description content]",
    "style": "style",
    "script": "script",
    "script:src": "script[src]",
    "img": "img[src alt]/",
    "img:s|img:srcset": "img[srcset src alt]",
    "img:z|img:sizes": "img[sizes srcset src alt]",
    "picture": "picture",
    "src|source": "source/",
    "src:sc|source:src": "source[src type]",
    "src:s|source:srcset": "source[srcset]",
    "src:t|source:type": "source[srcset type='${1:image/}']",
    "src:z|source:sizes": "source[sizes srcset]",
    "src:m|source:media": "source[media='(${1:min-width: })' srcset]",
    "src:mt|source:media:type": "source:media[type='${2:image/}']",
    "src:mz|source:media:sizes": "source:media[sizes srcset]",
    "src:zt|source:sizes:type": "source[sizes srcset type='${1:image/}']",
    "iframe": "iframe[src frameborder=0]",
    "embed": "embed[src type]/",
    "object": "object[data type]",
    "param": "param[name value]/",
    "map": "map[name]",
    "area": "area[shape coords href alt]/",
    "area:d": "area[shape=default]",
    "area:c": "area[shape=circle]",
    "area:r": "area[shape=rect]",
    "area:p": "area[shape=poly]",
    "form": "form[action]",
    "form:get": "form[method=get]",
    "form:post": "form[method=post]",
    "label": "label[for]",
    "input": "input[type=${1:text}]/",
    "inp": "input[name=${1} id=${1}]",
    "input:h|input:hidden": "input[type=hidden name]",
    "input:t|input:text": "inp[type=text]",
    "input:search": "inp[type=search]",
    "input:email": "inp[type=email]",
    "input:url": "inp[type=url]",
    "input:p|input:password": "inp[type=password]",
    "input:datetime": "inp[type=datetime]",
    "input:date": "inp[type=date]",
    "input:datetime-local": "inp[type=datetime-local]",
    "input:month": "inp[type=month]",
    "input:week": "inp[type=week]",
    "input:time": "inp[type=time]",
    "input:tel": "inp[type=tel]",
    "input:number": "inp[type=number]",
    "input:color": "inp[type=color]",
    "input:c|input:checkbox": "inp[type=checkbox]",
    "input:r|input:radio": "inp[type=radio]",
    "input:range": "inp[type=range]",
    "input:f|input:file": "inp[type=file]",
    "input:s|input:submit": "input[type=submit value]",
    "input:i|input:image": "input[type=image src alt]",
    "input:b|input:btn|input:button": "input[type=button value]",
    "input:reset": "input:button[type=reset]",
    "isindex": "isindex/",
    "select": "select[name=${1} id=${1}]",
    "select:d|select:disabled": "select[disabled.]",
    "opt|option": "option[value]",
    "textarea": "textarea[name=${1} id=${1} cols=${2:30} rows=${3:10}]",
    "marquee": "marquee[behavior direction]",
    "menu:c|menu:context": "menu[type=context]",
    "menu:t|menu:toolbar": "menu[type=toolbar]",
    "video": "video[src]",
    "audio": "audio[src]",
    "html:xml": "html[xmlns=http://www.w3.org/1999/xhtml]",
    "keygen": "keygen/",
    "command": "command/",
    "btn:s|button:s|button:submit": "button[type=submit]",
    "btn:r|button:r|button:reset": "button[type=reset]",
    "btn:d|button:d|button:disabled": "button[disabled.]",
    "fst:d|fset:d|fieldset:d|fieldset:disabled": "fieldset[disabled.]",
    "bq": "blockquote",
    "fig": "figure",
    "figc": "figcaption",
    "pic": "picture",
    "ifr": "iframe",
    "emb": "embed",
    "obj": "object",
    "cap": "caption",
    "colg": "colgroup",
    "fst": "fieldset",
    "btn": "button",
    "optg": "optgroup",
    "tarea": "textarea",
    "leg": "legend",
    "sect": "section",
    "art": "article",
    "hdr": "header",
    "ftr": "footer",
    "adr": "address",
    "dlg": "dialog",
    "str": "strong",
    "prog": "progress",
    "mn": "main",
    "tem": "template",
    "fset": "fieldset",
    "datag": "datagrid",
    "datal": "datalist",
    "kg": "keygen",
    "out": "output",
    "det": "details",
    "sum": "summary",
    "cmd": "command",
    "ri:d|ri:dpr": "img:s",
    "ri:v|ri:viewport": "img:z",
    "ri:a|ri:art": "pic>src:m+img",
    "ri:t|ri:type": "pic>src:t+img",
    "!!!": "{<!DOCTYPE html>}",
    "doc": "html[lang=${lang}]>(head>meta[charset=${charset}]+meta[http-equiv='X-UA-Compatible'][content='IE=edge']+meta:vp+title{${1:Document}})+body",
    "!|html:5": "!!!+doc",
    "c": "{<!-- ${0} -->}",
    "cc:ie": "{<!--[if IE]>${0}<![endif]-->}",
    "cc:noie": "{<!--[if !IE]><!-->${0}<!--<![endif]-->}"
  };
  var stylesheetSnippets = {
    "@f": "@font-face {\n	font-family: ${1};\n	src: url(${2});\n}",
    "@ff": "@font-face {\n	font-family: '${1:FontName}';\n	src: url('${2:FileName}.eot');\n	src: url('${2:FileName}.eot?#iefix') format('embedded-opentype'),\n		 url('${2:FileName}.woff') format('woff'),\n		 url('${2:FileName}.ttf') format('truetype'),\n		 url('${2:FileName}.svg#${1:FontName}') format('svg');\n	font-style: ${3:normal};\n	font-weight: ${4:normal};\n}",
    "@i|@import": "@import url(${0});",
    "@kf": "@keyframes ${1:identifier} {\n	${2}\n}",
    "@m|@media": "@media ${1:screen} {\n	${0}\n}",
    "ac": "align-content:start|end|flex-start|flex-end|center|space-between|space-around|stretch|space-evenly",
    "ai": "align-items:start|end|flex-start|flex-end|center|baseline|stretch",
    "anim": "animation:${1:name} ${2:duration} ${3:timing-function} ${4:delay} ${5:iteration-count} ${6:direction} ${7:fill-mode}",
    "animdel": "animation-delay:time",
    "animdir": "animation-direction:normal|reverse|alternate|alternate-reverse",
    "animdur": "animation-duration:${1:0}s",
    "animfm": "animation-fill-mode:both|forwards|backwards",
    "animic": "animation-iteration-count:1|infinite",
    "animn": "animation-name",
    "animps": "animation-play-state:running|paused",
    "animtf": "animation-timing-function:linear|ease|ease-in|ease-out|ease-in-out|cubic-bezier(${1:0.1}, ${2:0.7}, ${3:1.0}, ${3:0.1})",
    "ap": "appearance:none",
    "as": "align-self:start|end|auto|flex-start|flex-end|center|baseline|stretch",
    "b": "bottom",
    "bd": "border:${1:1px} ${2:solid} ${3:#000}",
    "bdb": "border-bottom:${1:1px} ${2:solid} ${3:#000}",
    "bdbc": "border-bottom-color:${1:#000}",
    "bdbi": "border-bottom-image:url(${0})",
    "bdbk": "border-break:close",
    "bdbli": "border-bottom-left-image:url(${0})|continue",
    "bdblrs": "border-bottom-left-radius",
    "bdbri": "border-bottom-right-image:url(${0})|continue",
    "bdbrrs": "border-bottom-right-radius",
    "bdbs": "border-bottom-style",
    "bdbw": "border-bottom-width",
    "bdc": "border-color:${1:#000}",
    "bdci": "border-corner-image:url(${0})|continue",
    "bdcl": "border-collapse:collapse|separate",
    "bdf": "border-fit:repeat|clip|scale|stretch|overwrite|overflow|space",
    "bdi": "border-image:url(${0})",
    "bdl": "border-left:${1:1px} ${2:solid} ${3:#000}",
    "bdlc": "border-left-color:${1:#000}",
    "bdlen": "border-length",
    "bdli": "border-left-image:url(${0})",
    "bdls": "border-left-style",
    "bdlw": "border-left-width",
    "bdr": "border-right:${1:1px} ${2:solid} ${3:#000}",
    "bdrc": "border-right-color:${1:#000}",
    "bdri": "border-right-image:url(${0})",
    "bdrs": "border-radius",
    "bdrst": "border-right-style",
    "bdrw": "border-right-width",
    "bds": "border-style:none|hidden|dotted|dashed|solid|double|dot-dash|dot-dot-dash|wave|groove|ridge|inset|outset",
    "bdsp": "border-spacing",
    "bdt": "border-top:${1:1px} ${2:solid} ${3:#000}",
    "bdtc": "border-top-color:${1:#000}",
    "bdti": "border-top-image:url(${0})",
    "bdtli": "border-top-left-image:url(${0})|continue",
    "bdtlrs": "border-top-left-radius",
    "bdtri": "border-top-right-image:url(${0})|continue",
    "bdtrrs": "border-top-right-radius",
    "bdts": "border-top-style",
    "bdtw": "border-top-width",
    "bdw": "border-width",
    "bfv": "backface-visibility:hidden|visible",
    "bg": "background:${1:#000}",
    "bga": "background-attachment:fixed|scroll",
    "bgbk": "background-break:bounding-box|each-box|continuous",
    "bgc": "background-color:#${1:fff}",
    "bgcp": "background-clip:padding-box|border-box|content-box|no-clip",
    "bgi": "background-image:url(${0})",
    "bgo": "background-origin:padding-box|border-box|content-box",
    "bgp": "background-position:${1:0} ${2:0}",
    "bgpx": "background-position-x",
    "bgpy": "background-position-y",
    "bgr": "background-repeat:no-repeat|repeat-x|repeat-y|space|round",
    "bgsz": "background-size:contain|cover",
    "bxsh": "box-shadow:${1:inset }${2:hoff} ${3:voff} ${4:blur} ${5:#000}|none",
    "bxsz": "box-sizing:border-box|content-box|border-box",
    "c": "color:${1:#000}",
    "cr": "color:rgb(${1:0}, ${2:0}, ${3:0})",
    "cra": "color:rgba(${1:0}, ${2:0}, ${3:0}, ${4:.5})",
    "cl": "clear:both|left|right|none",
    "cm": "/* ${0} */",
    "cnt": "content:'${0}'|normal|open-quote|no-open-quote|close-quote|no-close-quote|attr(${0})|counter(${0})|counters(${0})",
    "coi": "counter-increment",
    "colm": "columns",
    "colmc": "column-count",
    "colmf": "column-fill",
    "colmg": "column-gap",
    "colmr": "column-rule",
    "colmrc": "column-rule-color",
    "colmrs": "column-rule-style",
    "colmrw": "column-rule-width",
    "colms": "column-span",
    "colmw": "column-width",
    "cor": "counter-reset",
    "cp": "clip:auto|rect(${1:top} ${2:right} ${3:bottom} ${4:left})",
    "cps": "caption-side:top|bottom",
    "cur": "cursor:pointer|auto|default|crosshair|hand|help|move|pointer|text",
    "d": "display:block|none|flex|inline-flex|inline|inline-block|grid|inline-grid|subgrid|list-item|run-in|compact|table|inline-table|table-caption|table-column|table-column-group|table-header-group|table-footer-group|table-row|table-row-group|table-cell|ruby|ruby-base|ruby-base-group|ruby-text|ruby-text-group",
    "ec": "empty-cells:show|hide",
    "f": "font:${1:1em} ${2:sans-serif}",
    "fd": "font-display:auto|block|swap|fallback|optional",
    "fef": "font-effect:none|engrave|emboss|outline",
    "fem": "font-emphasize",
    "femp": "font-emphasize-position:before|after",
    "fems": "font-emphasize-style:none|accent|dot|circle|disc",
    "ff": "font-family:serif|sans-serif|cursive|fantasy|monospace",
    "fft": 'font-family:"Times New Roman", Times, Baskerville, Georgia, serif',
    "ffa": 'font-family:Arial, "Helvetica Neue", Helvetica, sans-serif',
    "ffv": "font-family:Verdana, Geneva, sans-serif",
    "fl": "float:left|right|none",
    "fs": "font-style:italic|normal|oblique",
    "fsm": "font-smoothing:antialiased|subpixel-antialiased|none",
    "fst": "font-stretch:normal|ultra-condensed|extra-condensed|condensed|semi-condensed|semi-expanded|expanded|extra-expanded|ultra-expanded",
    "fv": "font-variant:normal|small-caps",
    "fvs": "font-variation-settings:normal|inherit|initial|unset",
    "fw": "font-weight:normal|bold|bolder|lighter",
    "fx": "flex",
    "fxb": "flex-basis:fill|max-content|min-content|fit-content|content",
    "fxd": "flex-direction:row|row-reverse|column|column-reverse",
    "fxf": "flex-flow",
    "fxg": "flex-grow",
    "fxsh": "flex-shrink",
    "fxw": "flex-wrap:nowrap|wrap|wrap-reverse",
    "fsz": "font-size",
    "fsza": "font-size-adjust",
    "gtc": "grid-template-columns:repeat(${0})|minmax()",
    "gtr": "grid-template-rows:repeat(${0})|minmax()",
    "gta": "grid-template-areas",
    "gt": "grid-template",
    "gg": "grid-gap",
    "gcg": "grid-column-gap",
    "grg": "grid-row-gap",
    "gac": "grid-auto-columns:auto|minmax()",
    "gar": "grid-auto-rows:auto|minmax()",
    "gaf": "grid-auto-flow:row|column|dense|inherit|initial|unset",
    "gd": "grid",
    "gc": "grid-column",
    "gcs": "grid-column-start",
    "gce": "grid-column-end",
    "gr": "grid-row",
    "grs": "grid-row-start",
    "gre": "grid-row-end",
    "ga": "grid-area",
    "h": "height",
    "jc": "justify-content:start|end|stretch|flex-start|flex-end|center|space-between|space-around|space-evenly",
    "ji": "justify-items:start|end|center|stretch",
    "js": "justify-self:start|end|center|stretch",
    "l": "left",
    "lg": "background-image:linear-gradient(${1})",
    "lh": "line-height",
    "lis": "list-style",
    "lisi": "list-style-image",
    "lisp": "list-style-position:inside|outside",
    "list": "list-style-type:disc|circle|square|decimal|decimal-leading-zero|lower-roman|upper-roman",
    "lts": "letter-spacing:normal",
    "m": "margin",
    "mah": "max-height",
    "mar": "max-resolution",
    "maw": "max-width",
    "mb": "margin-bottom",
    "mih": "min-height",
    "mir": "min-resolution",
    "miw": "min-width",
    "ml": "margin-left",
    "mr": "margin-right",
    "mt": "margin-top",
    "ol": "outline",
    "olc": "outline-color:${1:#000}|invert",
    "olo": "outline-offset",
    "ols": "outline-style:none|dotted|dashed|solid|double|groove|ridge|inset|outset",
    "olw": "outline-width|thin|medium|thick",
    "op|opa": "opacity",
    "ord": "order",
    "ori": "orientation:landscape|portrait",
    "orp": "orphans",
    "ov": "overflow:hidden|visible|hidden|scroll|auto",
    "ovs": "overflow-style:scrollbar|auto|scrollbar|panner|move|marquee",
    "ovx": "overflow-x:hidden|visible|hidden|scroll|auto",
    "ovy": "overflow-y:hidden|visible|hidden|scroll|auto",
    "p": "padding",
    "pb": "padding-bottom",
    "pgba": "page-break-after:auto|always|left|right",
    "pgbb": "page-break-before:auto|always|left|right",
    "pgbi": "page-break-inside:auto|avoid",
    "pl": "padding-left",
    "pos": "position:relative|absolute|relative|fixed|static",
    "pr": "padding-right",
    "pt": "padding-top",
    "q": "quotes",
    "qen": "quotes:'\\201C' '\\201D' '\\2018' '\\2019'",
    "qru": "quotes:'\\00AB' '\\00BB' '\\201E' '\\201C'",
    "r": "right",
    "rsz": "resize:none|both|horizontal|vertical",
    "t": "top",
    "ta": "text-align:left|center|right|justify",
    "tal": "text-align-last:left|center|right",
    "tbl": "table-layout:fixed",
    "td": "text-decoration:none|underline|overline|line-through",
    "te": "text-emphasis:none|accent|dot|circle|disc|before|after",
    "th": "text-height:auto|font-size|text-size|max-size",
    "ti": "text-indent",
    "tj": "text-justify:auto|inter-word|inter-ideograph|inter-cluster|distribute|kashida|tibetan",
    "to": "text-outline:${1:0} ${2:0} ${3:#000}",
    "tov": "text-overflow:ellipsis|clip",
    "tr": "text-replace",
    "trf": "transform:${1}|skewX(${1:angle})|skewY(${1:angle})|scale(${1:x}, ${2:y})|scaleX(${1:x})|scaleY(${1:y})|scaleZ(${1:z})|scale3d(${1:x}, ${2:y}, ${3:z})|rotate(${1:angle})|rotateX(${1:angle})|rotateY(${1:angle})|rotateZ(${1:angle})|translate(${1:x}, ${2:y})|translateX(${1:x})|translateY(${1:y})|translateZ(${1:z})|translate3d(${1:tx}, ${2:ty}, ${3:tz})",
    "trfo": "transform-origin",
    "trfs": "transform-style:preserve-3d",
    "trs": "transition:${1:prop} ${2:time}",
    "trsde": "transition-delay:${1:time}",
    "trsdu": "transition-duration:${1:time}",
    "trsp": "transition-property:${1:prop}",
    "trstf": "transition-timing-function:${1:fn}",
    "tsh": "text-shadow:${1:hoff} ${2:voff} ${3:blur} ${4:#000}",
    "tt": "text-transform:uppercase|lowercase|capitalize|none",
    "tw": "text-wrap:none|normal|unrestricted|suppress",
    "us": "user-select:none",
    "v": "visibility:hidden|visible|collapse",
    "va": "vertical-align:top|super|text-top|middle|baseline|bottom|text-bottom|sub",
    "w": "width",
    "whs": "white-space:nowrap|pre|pre-wrap|pre-line|normal",
    "whsc": "white-space-collapse:normal|keep-all|loose|break-strict|break-all",
    "wid": "widows",
    "wm": "writing-mode:lr-tb|lr-tb|lr-bt|rl-tb|rl-bt|tb-rl|tb-lr|bt-lr|bt-rl",
    "wob": "word-break:normal|keep-all|break-all",
    "wos": "word-spacing",
    "wow": "word-wrap:none|unrestricted|suppress|break-word|normal",
    "z": "z-index",
    "zom": "zoom:1"
  };
  var xslSnippets = {
    "tm|tmatch": "xsl:template[match mode]",
    "tn|tname": "xsl:template[name]",
    "call": "xsl:call-template[name]",
    "ap": "xsl:apply-templates[select mode]",
    "api": "xsl:apply-imports",
    "imp": "xsl:import[href]",
    "inc": "xsl:include[href]",
    "ch": "xsl:choose",
    "wh|xsl:when": "xsl:when[test]",
    "ot": "xsl:otherwise",
    "if": "xsl:if[test]",
    "par": "xsl:param[name]",
    "pare": "xsl:param[name select]",
    "var": "xsl:variable[name]",
    "vare": "xsl:variable[name select]",
    "wp": "xsl:with-param[name select]",
    "key": "xsl:key[name match use]",
    "elem": "xsl:element[name]",
    "attr": "xsl:attribute[name]",
    "attrs": "xsl:attribute-set[name]",
    "cp": "xsl:copy[select]",
    "co": "xsl:copy-of[select]",
    "val": "xsl:value-of[select]",
    "for|each": "xsl:for-each[select]",
    "tex": "xsl:text",
    "com": "xsl:comment",
    "msg": "xsl:message[terminate=no]",
    "fall": "xsl:fallback",
    "num": "xsl:number[value]",
    "nam": "namespace-alias[stylesheet-prefix result-prefix]",
    "pres": "xsl:preserve-space[elements]",
    "strip": "xsl:strip-space[elements]",
    "proc": "xsl:processing-instruction[name]",
    "sort": "xsl:sort[select order]",
    "choose": "xsl:choose>xsl:when+xsl:otherwise",
    "xsl": "!!!+xsl:stylesheet[version=1.0 xmlns:xsl=http://www.w3.org/1999/XSL/Transform]>{\n|}",
    "!!!": '{<?xml version="1.0" encoding="UTF-8"?>}'
  };
  var pugSnippets = {
    "!!!": "{doctype html}"
  };
  var variables = {
    "lang": "en",
    "locale": "en-US",
    "charset": "UTF-8",
    "indentation": "	",
    "newline": "\n"
  };
  var defaultSyntaxes = {
    markup: "html",
    stylesheet: "css"
  };
  var defaultOptions = {
    "inlineElements": [
      "a",
      "abbr",
      "acronym",
      "applet",
      "b",
      "basefont",
      "bdo",
      "big",
      "br",
      "button",
      "cite",
      "code",
      "del",
      "dfn",
      "em",
      "font",
      "i",
      "iframe",
      "img",
      "input",
      "ins",
      "kbd",
      "label",
      "map",
      "object",
      "q",
      "s",
      "samp",
      "select",
      "small",
      "span",
      "strike",
      "strong",
      "sub",
      "sup",
      "textarea",
      "tt",
      "u",
      "var"
    ],
    "output.indent": "	",
    "output.baseIndent": "",
    "output.newline": "\n",
    "output.tagCase": "",
    "output.attributeCase": "",
    "output.attributeQuotes": "double",
    "output.format": true,
    "output.formatLeafNode": false,
    "output.formatSkip": ["html"],
    "output.formatForce": ["body"],
    "output.inlineBreak": 3,
    "output.compactBoolean": false,
    "output.booleanAttributes": [
      "contenteditable",
      "seamless",
      "async",
      "autofocus",
      "autoplay",
      "checked",
      "controls",
      "defer",
      "disabled",
      "formnovalidate",
      "hidden",
      "ismap",
      "loop",
      "multiple",
      "muted",
      "novalidate",
      "readonly",
      "required",
      "reversed",
      "selected",
      "typemustmatch"
    ],
    "output.reverseAttributes": false,
    "output.selfClosingStyle": "html",
    "output.field": (index, placeholder) => placeholder,
    "output.text": (text2) => text2,
    "markup.href": true,
    "comment.enabled": false,
    "comment.trigger": ["id", "class"],
    "comment.before": "",
    "comment.after": "\n<!-- /[#ID][.CLASS] -->",
    "bem.enabled": false,
    "bem.element": "__",
    "bem.modifier": "_",
    "jsx.enabled": false,
    "stylesheet.keywords": ["auto", "inherit", "unset", "none"],
    "stylesheet.unitless": ["z-index", "line-height", "opacity", "font-weight", "zoom", "flex", "flex-grow", "flex-shrink"],
    "stylesheet.shortHex": true,
    "stylesheet.between": ": ",
    "stylesheet.after": ";",
    "stylesheet.intUnit": "px",
    "stylesheet.floatUnit": "em",
    "stylesheet.unitAliases": { e: "em", p: "%", x: "ex", r: "rem" },
    "stylesheet.json": false,
    "stylesheet.jsonDoubleQuotes": false,
    "stylesheet.fuzzySearchMinScore": 0
  };
  var defaultConfig = {
    type: "markup",
    syntax: "html",
    variables,
    snippets: {},
    options: defaultOptions
  };
  var syntaxConfig = {
    markup: {
      snippets: parseSnippets(markupSnippets)
    },
    xhtml: {
      options: {
        "output.selfClosingStyle": "xhtml"
      }
    },
    xml: {
      options: {
        "output.selfClosingStyle": "xml"
      }
    },
    xsl: {
      snippets: parseSnippets(xslSnippets),
      options: {
        "output.selfClosingStyle": "xml"
      }
    },
    jsx: {
      options: {
        "jsx.enabled": true
      }
    },
    pug: {
      snippets: parseSnippets(pugSnippets)
    },
    stylesheet: {
      snippets: parseSnippets(stylesheetSnippets)
    },
    sass: {
      options: {
        "stylesheet.after": ""
      }
    },
    stylus: {
      options: {
        "stylesheet.between": " ",
        "stylesheet.after": ""
      }
    }
  };
  function parseSnippets(snippets) {
    const result = {};
    Object.keys(snippets).forEach((k) => {
      for (const name of k.split("|")) {
        result[name] = snippets[k];
      }
    });
    return result;
  }
  function resolveConfig(config = {}, globals = {}) {
    const type = config.type || "markup";
    const syntax = config.syntax || defaultSyntaxes[type];
    return Object.assign(Object.assign(Object.assign({}, defaultConfig), config), {
      type,
      syntax,
      variables: mergedData(type, syntax, "variables", config, globals),
      snippets: mergedData(type, syntax, "snippets", config, globals),
      options: mergedData(type, syntax, "options", config, globals)
    });
  }
  function mergedData(type, syntax, key, config, globals = {}) {
    const typeDefaults = syntaxConfig[type];
    const typeOverride = globals[type];
    const syntaxDefaults = syntaxConfig[syntax];
    const syntaxOverride = globals[syntax];
    return Object.assign(Object.assign(Object.assign(Object.assign(Object.assign(Object.assign({}, defaultConfig[key]), typeDefaults && typeDefaults[key]), syntaxDefaults && syntaxDefaults[key]), typeOverride && typeOverride[key]), syntaxOverride && syntaxOverride[key]), config[key]);
  }
  function backwardScanner(text2, start = 0) {
    return { text: text2, start, pos: text2.length };
  }
  function sol(scanner) {
    return scanner.pos === scanner.start;
  }
  function peek$1(scanner, offset = 0) {
    return scanner.text.charCodeAt(scanner.pos - 1 + offset);
  }
  function previous(scanner) {
    if (!sol(scanner)) {
      return scanner.text.charCodeAt(--scanner.pos);
    }
  }
  function consume3(scanner, match) {
    if (sol(scanner)) {
      return false;
    }
    const ok = typeof match === "function" ? match(peek$1(scanner)) : match === peek$1(scanner);
    if (ok) {
      scanner.pos--;
    }
    return !!ok;
  }
  function consumeWhile(scanner, match) {
    const start = scanner.pos;
    while (consume3(scanner, match)) {
    }
    return scanner.pos < start;
  }
  function isQuote3(c) {
    return c === 39 || c === 34;
  }
  function consumeQuoted(scanner) {
    const start = scanner.pos;
    const quote2 = previous(scanner);
    if (isQuote3(quote2)) {
      while (!sol(scanner)) {
        if (previous(scanner) === quote2 && peek$1(scanner) !== 92) {
          return true;
        }
      }
    }
    scanner.pos = start;
    return false;
  }
  var bracePairs = {
    [91]: 93,
    [40]: 41,
    [123]: 125
  };
  function isHtml(scanner) {
    const start = scanner.pos;
    if (!consume3(scanner, 62)) {
      return false;
    }
    let ok = false;
    consume3(scanner, 47);
    while (!sol(scanner)) {
      consumeWhile(scanner, isWhiteSpace4);
      if (consumeIdent(scanner)) {
        if (consume3(scanner, 47)) {
          ok = consume3(scanner, 60);
          break;
        } else if (consume3(scanner, 60)) {
          ok = true;
          break;
        } else if (consume3(scanner, isWhiteSpace4)) {
          continue;
        } else if (consume3(scanner, 61)) {
          if (consumeIdent(scanner)) {
            continue;
          }
          break;
        } else if (consumeAttributeWithUnquotedValue(scanner)) {
          ok = true;
          break;
        }
        break;
      }
      if (consumeAttribute(scanner)) {
        continue;
      }
      break;
    }
    scanner.pos = start;
    return ok;
  }
  function consumeAttribute(scanner) {
    return consumeAttributeWithQuotedValue(scanner) || consumeAttributeWithUnquotedValue(scanner);
  }
  function consumeAttributeWithQuotedValue(scanner) {
    const start = scanner.pos;
    if (consumeQuoted(scanner) && consume3(scanner, 61) && consumeIdent(scanner)) {
      return true;
    }
    scanner.pos = start;
    return false;
  }
  function consumeAttributeWithUnquotedValue(scanner) {
    const start = scanner.pos;
    const stack = [];
    while (!sol(scanner)) {
      const ch = peek$1(scanner);
      if (isCloseBracket2(ch)) {
        stack.push(ch);
      } else if (isOpenBracket3(ch)) {
        if (stack.pop() !== bracePairs[ch]) {
          break;
        }
      } else if (!isUnquotedValue(ch)) {
        break;
      }
      scanner.pos--;
    }
    if (start !== scanner.pos && consume3(scanner, 61) && consumeIdent(scanner)) {
      return true;
    }
    scanner.pos = start;
    return false;
  }
  function consumeIdent(scanner) {
    return consumeWhile(scanner, isIdent);
  }
  function isIdent(ch) {
    return ch === 58 || ch === 45 || isAlpha2(ch) || isNumber2(ch);
  }
  function isAlpha2(ch) {
    ch &= ~32;
    return ch >= 65 && ch <= 90;
  }
  function isNumber2(ch) {
    return ch > 47 && ch < 58;
  }
  function isWhiteSpace4(ch) {
    return ch === 32 || ch === 9;
  }
  function isUnquotedValue(ch) {
    return !isNaN(ch) && ch !== 61 && !isWhiteSpace4(ch) && !isQuote3(ch);
  }
  function isOpenBracket3(ch) {
    return ch === 123 || ch === 40 || ch === 91;
  }
  function isCloseBracket2(ch) {
    return ch === 125 || ch === 41 || ch === 93;
  }
  var code = (ch) => ch.charCodeAt(0);
  var specialChars = "#.*:$-_!@%^+>/".split("").map(code);
  var defaultOptions$1 = {
    type: "markup",
    lookAhead: true,
    prefix: ""
  };
  function extractAbbreviation(line, pos = line.length, options = {}) {
    const opt2 = Object.assign(Object.assign({}, defaultOptions$1), options);
    pos = Math.min(line.length, Math.max(0, pos == null ? line.length : pos));
    if (opt2.lookAhead) {
      pos = offsetPastAutoClosed(line, pos, opt2);
    }
    let ch;
    const start = getStartOffset(line, pos, opt2.prefix || "");
    if (start === -1) {
      return void 0;
    }
    const scanner = backwardScanner(line, start);
    scanner.pos = pos;
    const stack = [];
    while (!sol(scanner)) {
      ch = peek$1(scanner);
      if (stack.includes(125)) {
        if (ch === 125) {
          stack.push(ch);
          scanner.pos--;
          continue;
        }
        if (ch !== 123) {
          scanner.pos--;
          continue;
        }
      }
      if (isCloseBrace(ch, opt2.type)) {
        stack.push(ch);
      } else if (isOpenBrace(ch, opt2.type)) {
        if (stack.pop() !== bracePairs[ch]) {
          break;
        }
      } else if (stack.includes(93) || stack.includes(125)) {
        scanner.pos--;
        continue;
      } else if (isHtml(scanner) || !isAbbreviation(ch)) {
        break;
      }
      scanner.pos--;
    }
    if (!stack.length && scanner.pos !== pos) {
      const abbreviation2 = line.slice(scanner.pos, pos).replace(/^[*+>^]+/, "");
      return {
        abbreviation: abbreviation2,
        location: pos - abbreviation2.length,
        start: options.prefix ? start - options.prefix.length : pos - abbreviation2.length,
        end: pos
      };
    }
  }
  function offsetPastAutoClosed(line, pos, options) {
    if (isQuote3(line.charCodeAt(pos))) {
      pos++;
    }
    while (isCloseBrace(line.charCodeAt(pos), options.type)) {
      pos++;
    }
    return pos;
  }
  function getStartOffset(line, pos, prefix) {
    if (!prefix) {
      return 0;
    }
    const scanner = backwardScanner(line);
    const compiledPrefix = prefix.split("").map(code);
    scanner.pos = pos;
    let result;
    while (!sol(scanner)) {
      if (consumePair(scanner, 93, 91) || consumePair(scanner, 125, 123)) {
        continue;
      }
      result = scanner.pos;
      if (consumeArray(scanner, compiledPrefix)) {
        return result;
      }
      scanner.pos--;
    }
    return -1;
  }
  function consumePair(scanner, close, open) {
    const start = scanner.pos;
    if (consume3(scanner, close)) {
      while (!sol(scanner)) {
        if (consume3(scanner, open)) {
          return true;
        }
        scanner.pos--;
      }
    }
    scanner.pos = start;
    return false;
  }
  function consumeArray(scanner, arr) {
    const start = scanner.pos;
    let consumed = false;
    for (let i = arr.length - 1; i >= 0 && !sol(scanner); i--) {
      if (!consume3(scanner, arr[i])) {
        break;
      }
      consumed = i === 0;
    }
    if (!consumed) {
      scanner.pos = start;
    }
    return consumed;
  }
  function isAbbreviation(ch) {
    return ch > 64 && ch < 91 || ch > 96 && ch < 123 || ch > 47 && ch < 58 || specialChars.includes(ch);
  }
  function isOpenBrace(ch, syntax) {
    return ch === 40 || syntax === "markup" && (ch === 91 || ch === 123);
  }
  function isCloseBrace(ch, syntax) {
    return ch === 41 || syntax === "markup" && (ch === 93 || ch === 125);
  }
  function expandAbbreviation(abbr, config) {
    const resolvedConfig = resolveConfig(config);
    return resolvedConfig.type === "stylesheet" ? stylesheet(abbr, resolvedConfig) : markup(abbr, resolvedConfig);
  }
  function markup(abbr, config) {
    return stringify2(parse2(abbr, config), config);
  }
  function stylesheet(abbr, config) {
    return css(parse$1(abbr, config), config);
  }
  var emmet_es_default = expandAbbreviation;
  return __toCommonJS(emmet_es_exports);
})();
/* CUSTOM EMMET.NET CODE STARTS HERE */

var dotNetConfig = {
    options: {
        'output.compactBoolean': true,
        'output.field': (index, placeholder) => '{' + placeholder + '}'
    }
};

function loadPreferences(config) {
    // Restore my preferences if not overriden
    config.options = config.options || {};
    config.options['output.indent'] = config.options['output.indent'] || '  ';
    config.options['output.compactBoolean'] = config.options['output.compactBoolean'] || true;
    if (!config.options['output.field']) {
        config.options['output.field'] = (index, placeholder) => '{' + placeholder + '}';
    }

    dotNetConfig = config;
}

function replaceAbbreviation(codeLine, caretPos, contentType, contentToWrap, prefix) {
    var abbreviation = emmet.extract(codeLine, caretPos, { type: contentType, prefix: prefix});
    if (!abbreviation) {
        return false;
    }

    dotNetConfig.text = contentToWrap;
    dotNetConfig.type = contentType;
    if (prefix) {
        dotNetConfig.options['jsx.enabled'] = true;
    } else {
        dotNetConfig.options['jsx.enabled'] = false;
    }

    var replacement = emmet.default(abbreviation.abbreviation, dotNetConfig);
    var retVal = codeLine.slice(0, abbreviation.start) + replacement + codeLine.slice(abbreviation.end); 

    return retVal;
}