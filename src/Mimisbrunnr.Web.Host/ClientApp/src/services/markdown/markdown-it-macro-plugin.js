function macroPlugin(md) {
    function replaceMacro(state, start, end, silent) {
      const tail = state.src.slice(end);
      const regex = /\{\{macro:name=([^|]+)\|id=([^|]+)\|([^}]*)\}\}/g;
      
      // Проверяем, есть ли хотя бы один макрос
      if (!regex.test(tail)) {
        return false;
      }
      
      let match;
      let lastIndex = 0;
      let found = false;

      // Сбрасываем lastIndex для повторного использования регулярного выражения
      regex.lastIndex = 0;
      
      while ((match = regex.exec(tail)) !== null) {
        // Добавляем текст перед макросом
        if (match.index > lastIndex) {
          const text = tail.slice(lastIndex, match.index);
          if (!silent) {
            const textToken = state.push('text', '', 0);
            textToken.content = text;
          }
        }

        if (!silent) {
          const token = state.push('html_inline', '', 0);
          const [, name, id, params] = match;

          const paramArray = params.split('|');
          const paramKeyValuePairs = paramArray.map(param => param.split('='));
          const paramAttributes = paramKeyValuePairs.map(([key, value]) => `${key}=${value}`).join('&');
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

      // Добавляем оставшийся текст после последнего макроса
      if (lastIndex < tail.length) {
        const text = tail.slice(lastIndex);
        if (!silent) {
          const textToken = state.push('text', '', 0);
          textToken.content = text;
        }
      }

      return found;
    }
  
    md.inline.ruler.after('emphasis', 'macro_replace', replaceMacro);
  }
  
  export default macroPlugin;
