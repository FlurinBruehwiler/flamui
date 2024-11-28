# UI Elements

In flamui the entire UI is a tree of UI Elements. There are a default set of UI Elements, these include:
- Div
- Text
- SVG
- IMG

You can always create your own custom elements, but this should ideally not be necessary.

## Div
The div element serves many purposes, these include:
- Drawing simple Rectangles (including Borders, Corner Radius, etc) ([see](./Div/DivStyle.md))
- Clickable / Hoverable ([see](./Div/DivInput.md))
- Layouting children ([see](./Div/Layouting.md))
- Scroll content ([see](./Div/Scrolling.md))

## Text
The text element can render text in various forms:
- Single line
- Multi line
- Editable text (Input)

The text element doesn't support richt text.

## SVG
Used to render simple SVGs.

## IMG
Used to render different bitmap formats