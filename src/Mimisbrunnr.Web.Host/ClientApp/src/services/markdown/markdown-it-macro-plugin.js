const macroRegex = /{{macro:name=([^&]+)&id=([^&]+)&([^}]+)}}/g;

function macroPlugin(md) {
  // Создаем новый тип токена
  const macroToken = (state, startLine, endLine, silent) => {
    let pos = state.bMarks[startLine] + state.tShift[startLine];
    let max = state.eMarks[startLine];

    if (pos < max) {
        let match = macroRegex.exec(state.src.slice(pos));

        if(!match) 
            return false;

        const [, name, id, params] = match;
        const ariaParams = params.replace(/&/g, "&amp;");
        const token = new state.Token("macro", `div id="macro_${id}" aria-name="${name}" aria-params="${ariaParams}"></div`, 0);
        token.block = true;
        token.content = match[0];
        token.markup = match[0];
        token.map = [startLine, state.bMarks[startLine] + pos, startLine, state.bMarks[startLine] + pos + match[0].length];

        state.src = state.src.slice(0, pos) + state.src.slice(pos + match[0].length);
        state.tShift[startLine] += -match[0].length;
        for (let i = startLine + 1; i <= endLine; i++) {
            state.tShift[i] += -match[0].length;
        }

        if (!silent) {
            state.tokens.push(token);
        }

        return true;
    }

    return false;
  };

  // Зарегистрируем новый тип токена в markdown-it
  md.block.tokenizer.register("macro", macroToken, "paragraph", "blockquote", "list", "html_block");
}

export default macroPlugin;