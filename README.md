# TreeSlides
An automatic presentation generator from a TreeSheets hierarchy.

Turn this TreeSheets hierarchy:

![TreeSheets](ts.png?raw=true "TreeSheets")


Into this presentation (.gif file - wait for load):

![Demo](TreeSlides.gif?raw=true "Demo")

# Intro:
[TreeSheets](http://strlen.com/treesheets) is a superb tool for editing & describing hierarchies.

TreeSlides automatically generates an hierarchical presentation from a TreeSheets file exported to XML.  
The presentation can be navigated using the keyboard.

# Instructions:
1. Design your presentation in a linear-hierarchical manner:
  1. Each node (cell) can have text, and an optional child grid.
  2. Each child grid can have an arbitrary number of rows.
  3. Each row can have an arbitrary number of columns, but only the first column is displayed.
  4. Unlimited nesting is supported.
2. Export the treesheet to an XML file named topics.xml.
3. After compiling TreeSlides, place the xml near the TreeSlides.exe binary.
4. Optionally replace the bg.png file near the binary with another background image, having the same name.
5. To start the presetation, simply launch TreeSlides.exe.

# Presentation control:
* Right/Left buttons: go to next/previous node; traverses the tree in depth-first order
* Esc: go up one level
* Down/Up: go to next/previous sibling
* Home: restart presentation (go to root)
* F11: toggle full screen
* F5: reload from xml

# Example
An example presentation is provided in example.cts.
