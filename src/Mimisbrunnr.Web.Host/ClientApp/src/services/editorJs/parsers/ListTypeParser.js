import {parseTextFromMarkdown, replaceTextModifiersHtmlToMarkdown} from './TextParser';
export function parseListToMarkdown(blocks) {
    let items = {};
    switch (blocks.style) {
      case 'unordered':
        items = blocks.items.map((item) => (`* ${replaceTextModifiersHtmlToMarkdown(item)}`));
  
        return items.join('\n');
      case 'ordered':
        items = blocks.items.map((item, index) => (`${index + 1}. ${replaceTextModifiersHtmlToMarkdown(item)}`));
  
        return items.join('\n');
      default:
        break;
    }
  }
  
  export function parseMarkdownToList(blocks) {
    let listData = {};
    const itemData = [];
  
    blocks.children.forEach((items) => {
      items.children.forEach((listItem) => {
        var results = parseTextFromMarkdown(listItem).filter(x => x.data && x.data.text != null && x.data.text.trim() != "");
        if(results.length > 0 && results[0].data )
            itemData.push(results[0].data.text);
      });
    });
  
    listData = {
      data: {
        items: itemData,
        style: blocks.ordered ? 'ordered' : 'unordered',
      },
      type: 'list',
    };
  
    return listData;
  }