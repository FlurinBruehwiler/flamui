global using static Program;
using Flamui;
using Flamui.Components;
using SwissSkillsTraining2;

public static class Program
{
    public static InMemoryDb Data;
    public static FlamuiWindowHost windowHost;
    public static bool HasChanges;
    public static PhysicalWindow MainWindow;

    //I want a simple in memory model that I can modify to my liking, that is then saved to the db with a single function.
    public static async Task Main()
    {
        Data = new InMemoryDb();
        await Data.LoadFromDb();

        windowHost = new FlamuiWindowHost();
        MainWindow = windowHost.CreateWindow("SwissSkills", BuildMainWindow);

        windowHost.Run();
    }

    private static void BuildMainWindow(Ui ui)
    {
        ui.CascadingValues.TextColor = C.White;

        using (ui.Rect().MainAlign(MAlign.Start).Padding(10).Gap(10))
        {
            using (ui.Rect().ScrollVertical().Gap(10))
            {
                foreach (var blog in Data.Blogs)
                {
                    using var _ = ui.CreateIdScope(blog.BlogId);

                    using (ui.Rect().Direction().ShrinkHeight().Color(C.Gray5).Padding(5).Rounded(5).Gap(10))
                    {
                        using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween))
                        {
                            ui.Text($"Blog: {blog.Url}").Size(20);

                            if (ui.SquareButton("delete"))
                            {
                                ui.RunAfterFrame(() => Data.Blogs.Remove(blog));
                                HasChanges = true;
                            }
                        }

                        using (ui.Rect().ShrinkHeight().Gap(10))
                        {
                            foreach (var post in blog.Posts)
                            {
                                using var _2 = ui.CreateIdScope(post.PostId);

                                using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal))
                                {
                                    using (ui.Rect().ShrinkHeight().Gap(5))
                                    {
                                        ui.Text($"Title: {post.Title}");
                                        ui.Text($"Content: {post.Content}");
                                    }

                                    if (ui.SquareButton("delete"))
                                    {
                                        ui.RunAfterFrame(() => blog.Posts.Remove(post));
                                        HasChanges = true;
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
                            windowHost.CreateWindow("Create Post", ui2 => BuildCreatePostWindow(ui2, blog), new FlamuiWindowOptions
                            {
                                ParentWindow = MainWindow
                            });
                        }
                    }
                }

                if (Data.Blogs.Count == 0)
                {
                    ui.Text("No Blogs");
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
                    HasChanges = true;
                }

                if (ui.Button("Save", primary: true, disabled: !HasChanges))
                {
                    var err = Data.SaveToDb().GetAwaiter().GetResult(); //todo better solution
                    if (!string.IsNullOrEmpty(err))
                        ShowErrorDialog(err);
                    else
                        HasChanges = false;
                }
            }
        }
    }

    private static void ShowErrorDialog(string err)
    {
        windowHost.CreateWindow("An error occured", ui => { ui.Text(err).Multiline().Color(C.White); }, new FlamuiWindowOptions
        {
            ParentWindow = MainWindow
        });
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
                    HasChanges = true;
                    ui.Tree.UiTreeHost.CloseWindow();
                }
            }
        }
    }
}