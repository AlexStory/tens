module Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop
open Elmish

importAll "./styles/global.scss"

App.render
|> Program.run