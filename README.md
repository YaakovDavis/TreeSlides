# TreeSlides
An automatic presentation generator from a TreeSheets hierarchy

# Intro:
TreeSheets is a superb tool for editing & describing hierarchies: http://strlen.com/treesheets/

This tool automatically generates an hierarchical presentation from a TreeSheets file exported to XML.

# Instructions:
1. Design your presentation in an linear-hierarchical manner:
  1. Each node (cell) can have text, and an optional child grid.
  2. Each child grid can have an arbitrary number of rows.
  3. Each row can have an arbitrary number of columns, but only the first column is displayed.
  4. Unlimited nesting is supported.
2. Export the treesheet to an XML file named topics.xml.
3. After compiling TreeSlides, place the xml near the TreeSlides.exe binary.
4. Optionally replace the bg.png file near the binary with another background image, having the same name.
5. To start the presetation, simply launch TreeSlides.exe.

# Presentation control:
* Right/Left buttons: forward/backward in the presentation.
* Esc: Go up one level
* Down/Up: go to next/previous sibling.
* Home: restart presentation.
* F11: toggle full screen.
* F5: reload from xml.

# Example
An example presentation is provided in example.cts.