import CodeMirror from 'codemirror';

CodeMirror.defineMode('markdown_with_macro', function (config) {
  // Basic markdown mode (the one SimpleMDE already uses)
  var markdownMode = CodeMirror.getMode(config, 'markdown');

  var macroOverlay = {
    // token() is called for each position in the stream
    token: function (stream) {
      // If the thread is already inside the found macro -
      // just advance it to the end of the macro, otherwise return null.
      if (stream.match(/^{{macro:[^}]*}}/)) {
        // We found the full macro, return our token
        return 'macro-highlight';    // → .cm-macro-highlight in CSS
      }

      // Not interested - just eat one character,
      // to avoid an infinite loop
      stream.next();
      return null;
    }
  };

  // overlayMode connects two modes: base + our layer
  return CodeMirror.overlayMode(markdownMode, macroOverlay);
});