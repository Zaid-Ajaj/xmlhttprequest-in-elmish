module App

open Browser.Types
open Elmish
open Elmish.React
open Feliz
open Browser

type Request = { url: string; method: string; body: string }
type Response = { statusCode: int; body: string }

type Deferred<'t> =
  | HasNotStartedYet
  | InProgress
  | Resolved of 't

type AsyncOperationEvent<'t> =
  | Started
  | Finished of 't

type State = {
    LoremIpsum : Deferred<Result<string, string>>
}

type Msg =
    | LoadLoremIpsum of AsyncOperationEvent<Result<string, string>>

let init() = { LoremIpsum = HasNotStartedYet }, Cmd.ofMsg (LoadLoremIpsum Started)

let httpRequest (request: Request) (responseHandler: Response -> 'Msg) : Cmd<'Msg> =
    let command (dispatch: 'Msg -> unit) =
        // create an instance
        let xhr = XMLHttpRequest.Create()
        // open the connection
        xhr.``open``(method=request.method, url=request.url)
        // setup the event handler that triggers when the content is loaded
        xhr.onreadystatechange <- fun _ ->
            if xhr.readyState = ReadyState.Done
            then
              // create the response
              let response = { statusCode = xhr.status; body = xhr.responseText }
              // transform response into a message
              let messageToDispatch =  responseHandler response
              dispatch messageToDispatch

        // send the request
        xhr.send(request.body)

    Cmd.ofSub command

let update msg state =
    match msg with
    | LoadLoremIpsum Started ->
        let nextState = { state with LoremIpsum = InProgress }
        let request = { url = "/lorem-ipsum.txt"; method = "GET"; body = "" }
        let responseMapper (response: Response) =
            if response.statusCode = 200
            then LoadLoremIpsum (Finished (Ok response.body))
            else LoadLoremIpsum (Finished (Error "Could not load the content"))

        nextState, httpRequest request responseMapper

    | LoadLoremIpsum (Finished result) ->
        let nextState = { state with LoremIpsum = Resolved result }
        nextState, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    match state.LoremIpsum with
    | HasNotStartedYet ->
        Html.div ""

    | InProgress ->
        Html.div "Loading..."

    | Resolved (Ok content) ->
        Html.div [
            prop.style [ style.color.green ]
            prop.text content
        ]

    | Resolved (Error errorMsg) ->
        Html.div [
            prop.style [ style.color.red ]
            prop.text errorMsg
        ]

Program.mkProgram init update render
|> Program.withReactSynchronous "app"
|> Program.run