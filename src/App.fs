module App

open System
open Feliz
open Feliz.Bulma
open Elmish
open Elmish.React

type State =
    NotStarted
    | Running
    | Finished

type Model =
    {
        State: State
        Numbers: int list
        Score: int
        HighScore: int
        Buffer: int list
    }

let init() =
    {
        State = NotStarted
        Numbers = []
        Score = 0
        HighScore = 0
        Buffer = []
    }, Cmd.none

type Msg =
    StartGame
    | EndGame
    | NumberClicked of int
    | BufferAdded
    | Tick
    | AddNumber

let removeByIndex i list =
    list
    |> List.indexed
    |> List.filter (fun (idx, _) -> i <> idx)
    |> List.map snd

let update msg model =
    match msg with
    | StartGame ->
        { model with State = Running; Score = 0; Buffer = []; Numbers = [] }, Cmd.ofMsg Tick

    | EndGame ->
        { model with State = Finished }, Cmd.none

    | NumberClicked i ->
        let number = List.item i model.Numbers
        let buffer = number :: model.Buffer
        let numbers = removeByIndex i model.Numbers
        { model with
            Buffer = buffer
            Numbers = numbers
        }, Cmd.ofMsg BufferAdded

    | BufferAdded ->
        if model.Buffer.Length > 3 then
            model, Cmd.ofMsg EndGame
        elif List.sum model.Buffer > 10 then
            model, Cmd.ofMsg EndGame
        elif model.Buffer.Length = 3 && List.sum model.Buffer <> 10 then
            model, Cmd.ofMsg EndGame
        elif List.sum model.Buffer = 10 then
            let score = model.Score + (model.Buffer.Length - 1)
            let highScore = max score model.HighScore
            { model with Score = score; Buffer = []; HighScore = highScore }, Cmd.none
        else
            model, Cmd.none
    
    | AddNumber ->
        match model.Numbers.Length with
        | 10 -> model, Cmd.ofMsg EndGame
        | _ ->
            let r = Random().Next(1, 8)
            { model with Numbers = List.append model.Numbers [r] }, Cmd.none

    | Tick -> 
        match model.State with
        | Running ->
            let tick dispatch =
                let run = async {
                    dispatch AddNumber
                    do! Async.Sleep 1000
                    dispatch Tick
                }
                Async.StartImmediate run
            model, Cmd.ofSub tick
        | _       -> model, Cmd.none


let renderNumber index (number: int) dispatch =
    Bulma.button.button [
        column.is1
        button.isFullWidth
        button.isLarge
        prop.className "column"
        prop.onClick (fun _ -> dispatch (NumberClicked index))
        prop.text number
    ]

let renderNotStarted dispatch =
    Html.div [
        prop.classes ["is-flex" ; "is-justify-content-center"]
        prop.children [
            Bulma.button.button [
                button.isLarge
                prop.onClick (fun _ -> dispatch StartGame)
                prop.text "Begin"
            ]
        ]
    ]

let renderRunning model dispatch =
    Html.div [
        Bulma.columns [
            yield Bulma.column [ 
                column.is1 
            ]
            for i, n in List.indexed(model.Numbers) do yield renderNumber i n dispatch
        ]
        Bulma.subtitle [
            text.hasTextCentered
            prop.text $"Score: {model.Score}"
        ] 
    ]

let renderFinished model dispatch =
    Html.div [
        prop.classes ["is-flex"; "is-align-items-center"; "is-flex-direction-column"]
        prop.children [
            Bulma.title "Game Over"
            Bulma.subtitle $"Score: %d{model.Score}"
            Bulma.subtitle $"High Score: %d{model.HighScore}"
            Bulma.button.button [
                prop.onClick (fun _ -> dispatch StartGame)
                prop.text "Restart"
            ]
        ]
    ]


let view model dispatch =
    Bulma.container [
        text.isFamilyPrimary
        spacing.mt6

        prop.children [
            Bulma.title [
                text.hasTextCentered
                prop.text "Tens!"
            ]

            match model.State with
            | NotStarted -> renderNotStarted dispatch
            | Running    -> renderRunning model dispatch
            | Finished   -> renderFinished model dispatch
        ]
    ]

let render =
    Program.mkProgram init update view
    |> Program.withReactSynchronous("feliz-app")
