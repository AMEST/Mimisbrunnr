export function parseTextFromMarkdown(blocks) {
    var result = []
    blocks.children.forEach((item) => {
        var lastParagraph = result.length > 0 && result[result.length - 1].type == 'paragraph'
            ? result[result.length - 1]
            : null;
        var paragraph = lastParagraph == null
            ? { data: { text: "" }, type: 'paragraph' }
            : lastParagraph;
        var prefix = (paragraph.data.text == "" ? "" : " ");
        switch (item.type) {
            case "image":
                result.push({
                    data: {
                        caption: item.alt,
                        stretched: false,
                        url: item.url,
                        withBackground: false,
                        withBorder: false,
                    },
                    type: 'image',
                })
                break;
            case "text":
            case "strong":
            case "delete":    
            case "emphasis":
                var tagFromType = typeToTag(item.type);
                var openTag = tagFromType == null ? "" : `<${tagFromType}>`;
                var closeTag = tagFromType == null ? "" : `</${tagFromType}>`;
                if (item.children == null) {
                    paragraph.data.text += prefix + openTag + replaceExtraMarkdownToHtml(item.value) + closeTag;
                    if (lastParagraph == null)
                        result.push(paragraph);
                } else {
                    let parsedChild = parseTextFromMarkdown(item);
                    if (parsedChild.length == 1 && parsedChild[0].type == 'paragraph')
                        paragraph.data.text += prefix + openTag + replaceExtraMarkdownToHtml(parsedChild[0].value) + closeTag;
                    if (parsedChild.length > 0) {
                        if (lastParagraph == null)
                            result.push(paragraph);
                        parsedChild.forEach((childItem => result.push(childItem)));
                    }
                }
                break;
            case "link":
                if (item.children == null) {
                    paragraph.data.text += prefix + `<a href="${item.url}">${item.url}</a>`;
                    if (lastParagraph == null)
                        result.push(paragraph);
                } else {
                    let parsedChild = parseTextFromMarkdown(item);
                    if (parsedChild.length == 1 && parsedChild[0].type == 'paragraph')
                        paragraph.data.text += prefix + `<a href="${item.url}">${replaceExtraMarkdownToHtml(parsedChild[0].value)}</a>`;
                    if (lastParagraph == null)
                        result.push(paragraph);
                }
                break;
        }
    });
    return result;
}

export function replaceExtraMarkdownToHtml(markdownText) {
    if(!markdownText)
        return markdownText;
    return markdownText
        .replace(/~~([^~]*)~~/gim, '<strike>$1</strike>')
        .replace(/==([^=]*)==/gim, '<mark>$1</mark>');
}

export function replaceTextModifiersHtmlToMarkdown(htmlText) {
    if(!htmlText)
        return htmlText;
    return htmlText
        .replace(/><(strong|b)>/igm, '> **').replace(/<\/(strong|b)></igm, '** <').replace(/<\/?(strong|b)>/igm, '**')
        .replace(/><(em|i)>/igm, '> _').replace(/<\/(em|i)></igm, '_ <').replace(/<\/?(em|i)>/igm, '_')
        .replace(/><(mark)>/igm, '> _').replace(/<\/(mark)></igm, '_ <').replace(/<\/?(mark)>/igm, '_')
        .replace(/><(strike)>/igm, '> _').replace(/<\/(strike)></igm, '_ <').replace(/<\/?(strike)>/igm, '_')
        .replace(/<a href="([^"]+)">([^<]+)<\/a>/igm, '[$2]($1)')
        .replace(/<a href="([^"]+)" title="([^"]+)">([^<]+)<\/a>/igm, '[$3]($1 "$2")');
}

function typeToTag(type) {
    switch (type) {
        case "strong":
            return "strong";
        case "emphasis":
            return "em";
        case "delete":
            return "del"
        default:
            return null;
    }
}