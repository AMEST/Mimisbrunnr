import {parseTextFromMarkdown, replaceTextModifiersHtmlToMarkdown} from './TextParser';
export function parseListToMarkdown(blocks) {
    let items = {};
    switch (blocks.style) {
      case 'unordered':
        items = blocks.items.map((item) => (`* ${replaceTextModifiersHtmlToMarkdown(item)}`));
  
        return items;
      case 'ordered':
        items = blocks.items.map((item, index) => (`${index + 1} ${replaceTextModifiersHtmlToMarkdown(item)}`));
  
        return items;
      default:
        break;
    }
  }
  
  export function parseMarkdownToList(blocks) {
    let listData = {};
    const itemData = [];
  
    blocks.children.forEach((items) => {
      items.children.forEach((listItem) => {
        itemData.push(parseTextFromMarkdown(listItem));
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