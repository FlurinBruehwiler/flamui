using Microsoft.EntityFrameworkCore;

namespace SwissSkillsTraining2;

public class InMemoryDb(BloggingContext bloggingContext)
{
    public List<Blog> Blogs = [];

    public async Task LoadFromDb()
    {
        Blogs = await bloggingContext.Blogs.Include(x => x.Posts).ToListAsync();
    }

    public async Task SaveToDb()
    {
        //first insert, then delete

        var blogsInDb = (await bloggingContext.Blogs.ToListAsync()).ToHashSet();
        var postsInDb = (await bloggingContext.Posts.ToListAsync()).ToHashSet();

        //insert added blogs
        foreach (var blog in Blogs)
        {
            if (!blogsInDb.Contains(blog))
            {
                bloggingContext.Blogs.Add(new Blog
                {
                    Url = blog.Url
                });
            }
        }

        //insert added posts
        foreach (var post in Blogs.SelectMany(x => x.Posts))
        {
            if (!postsInDb.Contains(post))
            {
                bloggingContext.Posts.Add(new Post()
                {
                    Content = post.Content,
                    Title = post.Title,
                    BlogId = post.BlogId
                });
            }
        }

        //delete removed post
        foreach (var post in postsInDb)
        {
            if (!Blogs.SelectMany(x => x.Posts).Contains(post))
            {
                bloggingContext.Posts.Remove(post);
            }
        }

        //delete removed blogs
        foreach (var blog in blogsInDb)
        {
            if (!Blogs.Contains(blog))
            {
                bloggingContext.Blogs.Remove(blog);
            }
        }

        await bloggingContext.SaveChangesAsync();
    }
}

public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer($"Data source=DESKTOP-FBR\\MSSQLSERVER02;initial catalog=SwissSkillsTest;trusted_connection=true");
    }
}

public class Blog : IEquatable<Blog>
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    public List<Post> Posts { get; } = new();

    public bool Equals(Blog? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return BlogId == other.BlogId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Blog)obj);
    }

    public override int GetHashCode()
    {
        return BlogId;
    }
}

public class Post : IEquatable<Post>
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }

    public bool Equals(Post? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return PostId == other.PostId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Post)obj);
    }

    public override int GetHashCode()
    {
        return PostId;
    }
}