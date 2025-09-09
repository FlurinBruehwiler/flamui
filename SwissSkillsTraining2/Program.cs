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
        Data = new InMemoryDb();
        await Data.LoadFromDb();

        windowHost = new FlamuiWindowHost();
        windowHost.CreateWindow("SwissSkills", BuildMainWindow);

        windowHost.Run();
    }

    private static void BuildMainWindow(Ui ui)
    {
        ui.CascadingValues.TextColor = C.White;

        using (ui.Rect().MainAlign(MAlign.Start).Padding(10).Gap(10))
        {
            using (ui.Rect().Direction(Dir.Horizontal))
            {
                using (ui.Rect().ScrollVertical().ShrinkHeight().Gap(10))
                {
                    foreach (var blog in Data.Blogs)
                    {
                        using var _ = ui.CreateIdScope(blog.BlogId);

                        using (ui.Rect().Direction().ShrinkHeight().Color(C.Gray5).Padding(5).Rounded(5))
                        {
                            using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween))
                            {
                                ui.Text($"Blog: {blog.Url}").Size(30);


                                if (ui.SquareButton("delete"))
                                {
                                    ui.RunAfterFrame(() => Data.Blogs.Remove(blog));
                                }
                            }

                            using (ui.Rect().Padding(10).ShrinkHeight().Gap(5))
                            {
                                foreach (var post in blog.Posts)
                                {
                                    using var _2 = ui.CreateIdScope(post.PostId);

                                    using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal))
                                    {
                                        using (ui.Rect().ShrinkHeight())
                                        {
                                            ui.Text($"Title: {post.Title}");
                                            ui.Text($"Content: {post.Content}");
                                        }

                                        if (ui.SquareButton("delete"))
                                        {
                                            ui.RunAfterFrame(() => blog.Posts.Remove(post));
                                        }
                                    }
                                }

                                if (blog.Posts.Count == 0)
                                {
                                    ui.Text("No Blog posts");
                                }
                            }

                            if (ui.Button("Add Post"))
                            {
                                windowHost.CreateWindow("Create Post", ui2 => BuildCreatePostWindow(ui2, blog));
                            }
                        }
                    }

                    if (Data.Blogs.Count == 0)
                    {
                        ui.Text("No Blogs");
                    }
                }
            }

            using (ui.Rect().Direction(Dir.Horizontal).ShrinkHeight().Gap(10))
            {
                if (ui.Button("Add Blog"))
                {
                    Data.Blogs.Add(new Blog
                    {
                        Url = "Hi",
                        BlogId = Random.Shared.Next()
                    });
                }

                if (ui.Button("Save", primary: true))
                {
                    var err = Data.SaveToDb().GetAwaiter().GetResult(); //todo better solution
                    if (!string.IsNullOrEmpty(err))
                        ShowErrorDialog(err);
                }
            }
        }
    }

    private static void ShowErrorDialog(string err)
    {
        windowHost.CreateWindow("An error occured", ui => { ui.Text(err).Multiline(true).Color(C.White); });
    }

    private static void BuildCreatePostWindow(Ui ui, Blog blog)
    {
        ui.CascadingValues.TextColor = C.White;

        ref string title = ref ui.GetString("");
        ref string content = ref ui.GetString("");

        using (ui.Rect().MainAlign(MAlign.SpaceBetween).Padding(10))
        {
            using (ui.Rect().Gap(10))
            {
                ui.StyledInput(ref title);
                ui.StyledInput(ref content);
            }

            using (ui.Rect().Direction(Dir.Horizontal).Gap(10).ShrinkHeight())
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
                        Content = content,
                        Blog = blog,
                        PostId = Random.Shared.Next()
                    });
                    ui.Tree.UiTreeHost.CloseWindow();
                }
            }
        }
    }
}