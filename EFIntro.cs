using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFIntro;

public class User
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }

    public List<Post> Posts { get; set; } = new List<Post>();
}

public class Blog
{
    public int BlogId { get; set; }
    public string? Url { get; set; }
    public string? Name { get; set; }

    public List<Post> Posts { get; set; } = new List<Post>();
}

public class Post
{
    public int PostId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int BlogId { get; set; }
    public int UserId { get; set; }

    public User? User { get; set; }
    public Blog? Blog { get; set; }
}

public class BloggingContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Post> Post { get; set; }
    public DbSet<Blog> Blog { get; set; }

    public string DbPath { get; }

    public BloggingContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = Path.Join(path, "blogging.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}

class Program
{
    static void Main()
    {
        using var db = new BloggingContext();
        Console.WriteLine($"Database path: {db.DbPath}.");

        var UserFromCsv = File.ReadAllLines("user.csv")
            .Skip(1)
            .Select(line => line.Split(','))
            .Select(fields => new User
            {
                UserId = int.Parse(fields[0]),
                Username = fields[1],
                Password = fields[2],
            })
            .ToList();

        var PostFromCsv = File.ReadAllLines("post.csv")
            .Skip(1)
            .Select(line => line.Split(','))
            .Select(fields => new Post
            {
                PostId = int.Parse(fields[0]),
                Title = fields[1],
                Content = fields[2],
                BlogId = int.Parse(fields[3]),
                UserId = int.Parse(fields[4])
            })
            .ToList();

        var BlogFromCsv = File.ReadAllLines("blog.csv")
            .Skip(1)
            .Select(line => line.Split(','))
            .Select(fields => new Blog
            {
                BlogId = int.Parse(fields[0]),
                Url = fields[1],
                Name = fields[2]
            })
            .ToList();



        using (var context = new BloggingContext())
        {
            context.Database.EnsureCreated();

            foreach (var blog in BlogFromCsv)
            {
                var existingBlog = context.Blog.Find(blog.BlogId);
                if (existingBlog != null)
                {
                    context.Entry(existingBlog).CurrentValues.SetValues(blog);
                }
                else
                {
                    context.Blog.Add(blog);
                }
            }

            foreach (var user in UserFromCsv)
            {
                var existingUser = context.User.Find(user.UserId);
                if (existingUser != null)
                {
                    context.Entry(existingUser).CurrentValues.SetValues(user);
                }
                else
                {
                    context.User.Add(user);
                }
            }

            context.SaveChanges();

            
            foreach (var post in PostFromCsv)
            {
                var existingPost = context.Post.Find(post.PostId);
                if (existingPost != null)
                {
                    context.Entry(existingPost).CurrentValues.SetValues(post);
                }
                else
                {
                    context.Post.Add(post);
                }
            }

            context.SaveChanges();
        }




        Console.WriteLine("Users:");
        foreach (var user in UserFromCsv)
        {
            Console.WriteLine($"User ID: {user.UserId}, Username: {user.Username}, Password: {user.Password}");

            foreach (var post in user.Posts)
            {
                Console.WriteLine($"  Post ID: {post.PostId}, Title: {post.Title}");
            }
        }

      
        Console.WriteLine("\nBlogs:");
        foreach (var blog in BlogFromCsv)
        {
            Console.WriteLine($"Blog ID: {blog.BlogId}, Name: {blog.Name}, URL: {blog.Url}");

            foreach (var post in blog.Posts)
            {
                Console.WriteLine($"  Post ID: {post.PostId}, Title: {post.Title}");

                if (post.User != null)
                {
                    Console.WriteLine($"    User ID: {post.User.UserId}, Username: {post.User.Username}");
                }
            }
        }

      
        Console.WriteLine("\nPosts:");
        foreach (var post in PostFromCsv)
        {
            Console.WriteLine($"Post ID: {post.PostId}, Title: {post.Title}, Content: {post.Content}");

            if (post.User != null)
            {
                Console.WriteLine($"  User ID: {post.User.UserId}, Username: {post.User.Username}");
            }

            if (post.Blog != null)
            {
                Console.WriteLine($"  Blog ID: {post.Blog.BlogId}, Name: {post.Blog.Name}");
            }
        }


    }
}