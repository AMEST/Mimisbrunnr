function macroPlugin(md) {
    function replaceMacro(state, start, end, silent) {
      const tail = state.src.slice(end);
      const regex = /\{\{macro:name=([^|]+)\|id=([^|]+)\|([^}]*)\}\}/g;
      
      // Check if there is at least one macro
      if (!regex.test(tail)) {
        return false;
      }
      
      let match;
      let lastIndex = 0;
      let found = false;

      // Reset lastIndex to reuse the regular expression
      regex.lastIndex = 0;
      
      while ((match = regex.exec(tail)) !== null) {
        // Add text before the macro
        if (match.index > lastIndex) {
          const text = tail.slice(lastIndex, match.index);
          if (text.indexOf(state.pending) !== -1)
            state.pending = '';
          if (!silent && state.pending != text && !state.tokens.find(x => text.indexOf(x.content) !== -1)) {
            const textToken = state.push('text', '', 0);
            textToken.content = text;
          }
        }

        if (!silent) {
          const token = state.push('html_inline', '', 0);
          const [, name, id, params] = match;

          const paramAttributes = params.replaceAll('|', '&');
          const encodedParams = encodeURI(paramAttributes);

          token.block = true;
          token.content = `<div id="macro_${id}" class="mm-macro-block" aria-type="macros" aria-name="${name}" aria-params="${encodedParams}"></div>`;
          token.map = [start, state.pos];
          token.markup = '{{';
        }
        
        lastIndex = match.index + match[0].length;
        state.pos = state.pos + match.index + match[0].length;
        found = true;
      }

      // Add the remaining text after the last macro
      if (lastIndex < tail.length) {
        const text = tail.slice(lastIndex);
        if (!silent) {
          const textToken = state.push('text', '', 0);
          textToken.content = text;
          state.pos = tail.length;
        }
      }

      return found;
    }
  
    md.inline.ruler.after('emphasis', 'macro_replace', replaceMacro);
  }
  
  export default macroPlugin;
