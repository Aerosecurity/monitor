// Learn more about F# at http://fsharp.org
open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media
open System.Diagnostics
open System.ComponentModel
open System.Windows.Data
open System.IO
open FSharp.Data

let defaultSettingsFile = "default.json"
let settingsFile = "settings.json"
if not (File.Exists settingsFile) then
    File.Copy(defaultSettingsFile, settingsFile)
let settings = settingsFile 
            |> File.ReadAllText
            |> JsonValue.Parse
            
            

let cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true)

type CounterViewModel () =
    let ev = Event<_,_>()
    let mutable cpuPercent = ""
    member x.CpuPercent 
        with get() = cpuPercent
        and set(cpuPercent') =
            cpuPercent <- cpuPercent'
            ev.Trigger(x, PropertyChangedEventArgs("CpuPercent"))
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged: IEvent<PropertyChangedEventHandler,PropertyChangedEventArgs> = 
            ev.Publish

let cpuVM = CounterViewModel()

let cpuBinding = 
    let binding = Binding("CpuPercent")
    binding.Source <- cpuVM
    binding

let timer = new System.Timers.Timer(1000.0)
timer.Elapsed.Add(fun _ -> cpuVM.CpuPercent <- cpuCounter.NextValue().ToString())
timer.Enabled <- true
timer.Start()

let mainWindowContent() = 
    let mutable label = TextBlock()
    label.FontSize <- 22.0
    label.Foreground <- Brushes.White
    label.FontWeight <- FontWeights.Bold
    label.SetBinding(TextBlock.TextProperty, cpuBinding) |> ignore
    //cpuVM.CpuPercent <- cpuCounter.NextValue().ToString()
    let label2 = Label()
    label2.Content <- "Second text"
    label2.FontSize <- 32.0
    label2.Foreground <- Brushes.Green
    label2.FontWeight <- FontWeights.ExtraBold
    let stackLayout = StackPanel()
    stackLayout.Orientation <- Orientation.Vertical
    stackLayout.Children.Add(label) |> ignore
    stackLayout.Children.Add(label2) |> ignore
    stackLayout
    

type MainWindow() =
    inherit Window()
    do 
        base.WindowStyle <- WindowStyle.None
        base.AllowsTransparency <- true
        base.Background <- BrushConverter().ConvertFrom("#22000000") :?> SolidColorBrush
        base.Title <- "Hello World"
        base.Height <- 400.0
        base.Width <- 800.0
        base.Content <- mainWindowContent()//TextBlock(Text = "Hello World")//mainWindowContent

type App() =
    inherit Application()
    override x.OnStartup e = 
        x.MainWindow <- MainWindow()
        x.MainWindow.Show()

[<EntryPoint;STAThread>]
let main argv =
    App().Run() |> ignore
    0 // return an integer exit code
