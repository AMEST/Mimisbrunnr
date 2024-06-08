<template>
  <b-modal id="guide-modal" centered size="lg" ok-only lazy title="Guide"> 
    <vue-markdown :source="this.guide" :toc="true"/>
  </b-modal>
</template>

<script>
const VueMarkdown = () => import(/* webpackChunkName: "vue-markdown-component" */"@/thirdparty/VueMarkdown");
export default {
    name: "GuideModal",
    components: {
        VueMarkdown,
    },
    data() {
        return {
            guide: `
## Table of contents

@[toc]

\`\`\`
@[toc]
\`\`\`

**Working only from "## medium header" to "#### tiny header"**

## Text emphasis
**Bold**, *italic*, ~~strikethrough~~, ++Inserted text++, ==Marked text==
\`\`\`
**Bold**, *italic*, ~~strikethrough~~, ++Inserted text++, ==Marked text==
\`\`\`

## Headers
\`\`\`
# Big header
## Medium header
### Small header
#### Tiny header
\`\`\`

## Lists
\`\`\`
* Generic list item
* Generic list item
* Generic list item
\`\`\`

\`\`\`
1. Numbered list item
1. Numbered list item
1. Numbered list item
\`\`\`

## Links
\`\`\`
[Text to display](http://www.example.com)
\`\`\`

## Quotes
\`\`\`
> This is a quote.
> It can span multiple lines!
\`\`\`

## Images and image size
\`\`\`
![](http://www.example.com/image.jpg)

![](http://www.example.com/image.jpg =100x100) // image with size 100x100 px

![](http://www.example.com/image.jpg =50%x) // image with width 50% and height auto

![](http://www.example.com/image.jpg =x40%) // image with height 40% and width auto
\`\`\`

## Tables
\`\`\`
| Column 1 | Column 2 | Column 3 |
| -------- | -------- | -------- |
| John     | Doe      | Male     |
| Mary     | Smith    | Female   |
\`\`\`

## Display code

\`\`\`
\`var example = "hello!";\`

Or spanning multiple lines...

\\\`\\\`\\\`
var example = "hello!";
alert(example);
\\\`\\\`\\\`
\`\`\`

## Typographic replacements
(c) (C) (r) (R) (tm) (TM) (p) (P) +-

\`\`\`
(c) (C) (r) (R) (tm) (TM) (p) (P) +-
\`\`\`

## Horizontal Rules
***
\`\`\`
___

---

***
\`\`\`

## Emojies

:wink: :cry: :laughing: :yum:
:-) :-( 8-) ;)
\`\`\`
:wink: :cry: :laughing: :yum:
:-) :-( 8-) ;)
\`\`\`

## Subscript/Superscript
- 19^th^
- H~2~O

\`\`\`
- 19^th^
- H~2~O
\`\`\`

## Collapsable (spoiler)
+++ Click me!
Hello, world!
+++

\`\`\`
+++ Click me!
Hello, world!
+++
\`\`\`

## Custom containers
### Blocks
::: warning
*It's warning*
:::

\`\`\`
::: warning
*It's warning*
:::
\`\`\`

:::: danger
*It's danger*
::::

\`\`\`
:::: danger
*It's danger*
::::
\`\`\`

:::: success
*It's success*
::::

\`\`\`
:::: success
*It's success*
::::
\`\`\`

:::: info
*It's info*
::::

\`\`\`
:::: info
*It's info*
::::
\`\`\`


::::: card
:::: card-body
::: card-title
# This is title
:::
::: card-text
text of this cards
:::
::::
:::::
   
\`\`\`
::::: card
:::: card-body
::: card-title
# This is title
:::
::: card-text
text of this cards
:::
::::
:::::
\`\`\`

### Layout

::::: row
:::: col-8
*Col 1*
::: success
Green block
:::
::::
:::: col-4
*Col 2*

| Column 1 | Column 2 |
| -------- | -------- |
| Text     | Text     |
::::
:::::

\`\`\`
::::: row
:::: col-8
*Col 1*
::: success
Green block
:::
::::
:::: col-4
*Col 2*

| Column 1 | Column 2 |
| -------- | -------- |
| Text     | Text     |
::::
:::::
\`\`\`

### Alignment

:::: align-center
I am at center
::: align-right
I am at right
:::
::: align-left
I am at left
:::
::::

\`\`\`
:::: align-center
I am at center
::: align-right
I am at right
:::
::: align-left
I am at left
:::
::::
\`\`\`
            `
        }
    },
};
</script>

<style>
#guide-modal pre {
    display: block;
    padding: 9.5px;
    margin: 0 0 10px;
    font-size: 13px;
    line-height: 1.42857143;
    color: #333;
    word-break: break-all;
    word-wrap: break-word;
    background-color: #f5f5f5;
    border: 1px solid #ccc;
    border-radius: 4px;
}
</style>