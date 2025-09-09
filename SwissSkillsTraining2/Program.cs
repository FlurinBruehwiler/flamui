global using static Program;
using Flamui;
using Flamui.Components;
using SwissSkillsTraining2;

public static class Program
{
    public static InMemoryDb Data;
    public static FlamuiWindowHost windowHost;

    //I want a simple in memory model that I can modify to my liking, that is then saved to the db with a single function.
    public static async Task Main()
    {
        await using var db = new BloggingContext();
        Data = new InMemoryDb(db);
        await Data.LoadFromDb();

        windowHost = new FlamuiWindowHost();
        windowHost.CreateWindow("SwissSkills", BuildMainWindow);
    }

    private static void BuildMainWindow(Ui ui)
    {
        foreach (var blog in Data.Blogs)
        {
            using var _ = ui.CreateIdScope(blog.BlogId);

            using (ui.Rect().Direction())
            {
                ui.Text($"Blog: {blog.Url}").Size(30);
                using (ui.Rect().Padding(10))
                {
                    foreach (var post in blog.Posts)
                    {
                        ui.Text(post.Title);
                        ui.Text(post.Content);
                    }
                }

                if (ui.Button("Add Post"))
                {
                    windowHost.CreateWindow("Create Post", ui2 => BuildCreatePostWindow(ui2, blog));
                }
            }
        }
    }

    private static void BuildCreatePostWindow(Ui ui, Blog blog)
    {
        ref string title = ref ui.GetString("");
        ref string content = ref ui.GetString("");

        using (ui.Rect())
        {
            using (ui.Rect())
            {
                ui.StyledInput(ref title);
                ui.StyledInput(ref content);
            }

            using (ui.Rect().Direction(Dir.Horizontal))
            {
                if (ui.Button("Cancel"))
                {
                    ui.Tree.UiTreeHost.CloseWindow();
                }

                if (ui.Button("Ok", primary: true))
                {
                    blog.Posts.Add(new Post
                    {
                        Title = title,
                        Content = content
                    });
                    ui.Tree.UiTreeHost.CloseWindow();
                }
            }
        }
    }
}