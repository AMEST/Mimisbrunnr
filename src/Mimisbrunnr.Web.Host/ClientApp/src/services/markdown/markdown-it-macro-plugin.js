function macroPlugin(md) {
    function replaceMacro(state, start, end, silent) {
      const tail = state.src.slice(end);
      const match = tail.match(/^\{\{macro:name=([^\|]+)\|id=([^\|]+)\|([^}]+)\}\}/);
  
      if (match) {
        if (!silent) {
          const token = state.push('html_inline', '', 0);
          const [, name, id, params] = match;
  
          const paramArray = params.split('|');
          const paramKeyValuePairs = paramArray.map(param => param.split('='));
          const paramAttributes = paramKeyValuePairs.map(([key, value]) => `${key}="${value}"`).join('&');
          const encodedParams = encodeURI(paramAttributes);
  
          token.block = true;
          token.content = `<div id="macro_${id}" aria-name="${name}" aria-params="${encodedParams}"></div>`;
          token.map = [start, state.pos];
          token.markup = '{{'; // markup will be set to '{{' for proper handling
        }
        state.pos = state.pos + match.index + match[0].length;
        return true;
      }
      return false;
    }
  
    md.inline.ruler.after('emphasis', 'macro_replace', replaceMacro);
  }
  
  export default macroPlugin;