import showdown from 'showdown';

export function removeSpacesInsideTags(html) {
    return html.replace(/<([^>]+)>/g, (match, p1) => {
        return `<${p1.replace(/\s+/g, ' ').trim()}>`;
    });
}

//eslint-disable-next-line
export function sanitize(html, removeMeta, removeEmptyTags, removeAttribute, removeComments) {
    const metaCharsetRegex = /<meta[^>]*>/g;
    const emptyTagRegex = /<[^>]*>\s*<\/[^>]*>/g;
    const attributesRegex = /<[^>]*>/g;
    const commentRegex = /<!--[^>]*>/g;

    function removeAttributes(match) {
        return match.replace(/<[^>]*>/g, (tag) => {
            return tag.replace(/[^<>\s]+="[^"]*"/g, '');
        });
    }

    if(removeMeta)
        html = html.replace(metaCharsetRegex, '');
    if(removeEmptyTags)
        html = html.replace(emptyTagRegex, '');
    if(removeAttribute)
        html = removeSpacesInsideTags(html.replace(attributesRegex, removeAttributes));
    if(removeComments)
        html = html.replace(commentRegex, '');
    return html;
}

//eslint-disable-next-line
export function fixShowdownNodeSubParser(){
    showdown.subParser('makeMarkdown.node', function (node, globals, spansOnly) {
        'use strict';

        spansOnly = spansOnly || false;

        let txt = '';

        // edge case of text without wrapper paragraph
        if (node.nodeType === 3) {
          return showdown.subParser('makeMarkdown.txt')(node, globals);
        }

        // HTML comment
        if (node.nodeType === 8) {
          return '<!--' + node.data + '-->\n\n';
        }

        // process only node elements
        if (node.nodeType !== 1) {
          return '';
        }

        const tagName = node.tagName.toLowerCase();

        switch (tagName) {

          //
          // BLOCKS
          //
          case 'h1':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 1) + '\n\n'; }
            break;
          case 'h2':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 2) + '\n\n'; }
            break;
          case 'h3':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 3) + '\n\n'; }
            break;
          case 'h4':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 4) + '\n\n'; }
            break;
          case 'h5':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 5) + '\n\n'; }
            break;
          case 'h6':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.header')(node, globals, 6) + '\n\n'; }
            break;

          case 'p':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.paragraph')(node, globals) + '\n\n'; }
            break;

          case 'blockquote':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.blockquote')(node, globals) + '\n\n'; }
            break;

          case 'hr':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.hr')(node, globals) + '\n\n'; }
            break;

          case 'ol':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.list')(node, globals, 'ol') + '\n\n'; }
            break;

          case 'ul':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.list')(node, globals, 'ul') + '\n\n'; }
            break;

          case 'precode':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.codeBlock')(node, globals) + '\n\n'; }
            break;

          case 'pre':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.pre')(node, globals) + '\n\n'; }
            break;

          case 'table':
            if (!spansOnly) { txt = showdown.subParser('makeMarkdown.table')(node, globals) + '\n\n'; }
            break;

          //
          // SPANS
          //
          case 'code':
            txt = showdown.subParser('makeMarkdown.codeSpan')(node, globals);
            break;

          case 'em':
          case 'i':
            txt = showdown.subParser('makeMarkdown.emphasis')(node, globals);
            break;

          case 'strong':
          case 'b':
            txt = showdown.subParser('makeMarkdown.strong')(node, globals);
            break;

          case 'del':
            txt = showdown.subParser('makeMarkdown.strikethrough')(node, globals);
            break;

          case 'a':
            txt = showdown.subParser('makeMarkdown.links')(node, globals);
            break;

          case 'img':
            txt = showdown.subParser('makeMarkdown.image')(node, globals);
            break;
          case 'br':
            txt = node.outerHTML + '\n';
            break;
          default:
            txt = node.outerHTML + ' ';
        }
        return txt;
      });
}


export function htmlToMarkdown(html) {
    var sanitizedHtml = sanitize(html, true, false, true, false);
    fixShowdownNodeSubParser();
    var converter = new showdown.Converter();
    var markdown = converter.makeMarkdown(sanitizedHtml);
    return sanitize(markdown, false, true, false, true);
}