namespace TouchChanX.Shared

open System.Drawing

module Constants =
    [<Literal>]
    let TouchSpacing = 2

type TouchDockAnchor =
    | Left of Scale: float
    | Top of Scale: float
    | Right of Scale: float
    | Bottom of Scale: float
    | TopLeft
    | TopRight
    | BottomLeft
    | BottomRight
    with
        static member Default = Left 0.5
        
        static member FromRect(containerSize: Size, touchRect: Rectangle) =
            let spacing = int Constants.TouchSpacing
            
            let right = containerSize.Width - spacing - touchRect.Width
            let bottom = containerSize.Height - spacing - touchRect.Height
            
            let x = touchRect.X
            let y = touchRect.Y
            
            match x, y with
            | x, y when x = spacing && y = spacing -> TopLeft
            | x, y when x = spacing && y = bottom -> BottomLeft
            | x, y when x = right && y = spacing -> TopRight
            | x, y when x = right && y = bottom -> BottomRight
            
            | x, _ when x = spacing ->
                let scale = (float y + float spacing + float touchRect.Height / 2.0) / float containerSize.Height
                Left scale
            | _, y when y = spacing ->
                let scale = (float x + float spacing + float touchRect.Width / 2.0) / float containerSize.Width
                Top scale
            | x, _ when x = right ->
                let scale = (float y + float spacing + float touchRect.Height / 2.0) / float containerSize.Height
                Right scale
            | _, y when y = bottom ->
                let scale = (float x + float spacing + float touchRect.Width / 2.0) / float containerSize.Width
                Bottom scale
            
            | _ -> TouchDockAnchor.Default