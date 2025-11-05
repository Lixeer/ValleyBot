namespace SayWelcome;
class Builder
{
    public static void Build()
    {

    }
    public static object BuildImgMessage(string url)
    {
        return new[]
        {
        new
        {
            type = "image",
            data = new
            {
                file = url
            }
        }
    };
    }
    public static object BuildTextMessage(string text)
    {
        return new[]
        {
        new
        {
            type = "text",
            data = new
            {
                text = $"{text}"
            }
        }
    };
    }
}