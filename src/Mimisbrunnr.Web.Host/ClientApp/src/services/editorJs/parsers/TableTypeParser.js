import {parseTextFromMarkdown, replaceTextModifiersHtmlToMarkdown} from './TextParser';

export function parseTableToMarkdown(blocks) {
    let items = [];
    if(blocks.content.length == 0)
        return;
    var startWith = 0;
    var firstLineLength = blocks.content[0].length;
    if(blocks.withHeadings){
        startWith = 1;
        items.push(`|${replaceTextModifiersHtmlToMarkdown(blocks.content[0].join('|'))}|`);
        items.push(`|${[...Array(firstLineLength)].map(() => `--------`).join('|')}|`);
    }else{
        items.push(`|${[...Array(firstLineLength)].map(() => "").join('|')}|`);
        items.push(`|${[...Array(firstLineLength)].map(() => `--------`).join('|')}|`);
    }
    for(var i = startWith; i < firstLineLength; i ++)
        items.push(`|${replaceTextModifiersHtmlToMarkdown(blocks.content[i].join('|'))}|`);
    return items.join("\n");
  }
  
  export function parseMarkdownToTable(blocks) {
    let listData = {};
    const itemData = [];
    var emptyHeader = false;
  
    blocks.children.forEach((row) => {
      var rowData = [];
      row.children.forEach((cell) => {
        var cellData = "";
        cell.children.forEach((entry) => {
            cellData += entry.value;
        });
        if(itemData.length == 0)
            emptyHeader = cellData == null || cellData == "";
        rowData.push(cellData);
      });
      itemData.push(rowData);
    });
  
    listData = {
      data: {
        items: itemData,
        withHeadings: !emptyHeader,
        style: blocks.ordered ? 'ordered' : 'unordered',
      },
      type: 'table',
    };
  
    return listData;
  }