$PBExportHeader$w_test.srw
forward
global type w_test from window
end type
end forward

global type w_test from window
integer width = 4754
integer height = 1980
boolean titlebar = true
string title = "Untitled"
boolean controlmenu = true
boolean minbox = true
boolean maxbox = true
boolean resizable = true
long backcolor = 67108864
string icon = "AppIcon!"
boolean center = true
end type
global w_test w_test

type variables
nvo_ftpclientwrapper invo_ftpclientwrapper
end variables
on w_test.create
end on

on w_test.destroy
end on

event open;invo_ftpclientwrapper = create nvo_ftpclientwrapper
end event

