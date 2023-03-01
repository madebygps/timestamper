# timestamper cli tool

## Prerequesits

- [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)

## How to setup

1. Clone the code.
2. Move into the directory.
3. Set an environment variable with your OpenAI API Key

    ```sh
    export openai_api_key=your-key-value
    ````

4. Create a NuGet package by running the dotnet pack command:

    ```csharp
    dotnet pack
    ```

5. Install as a [global](https://learn.microsoft.com/dotnet/core/tools/global-tools-how-to-use) or [local tool](https://learn.microsoft.com/dotnet/core/tools/local-tools-how-to-use)

## How to use

Once it's setup, you can use the tool running:

```sh
timestamper <youtube_video_url> number_of_timestamps_to_generate
```
as an example
```
timestamper https://www.youtube.com/watch?v=u2mUpkApObk 2