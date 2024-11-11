using System;
using System.Linq;
using NLog;


class Program
{
  static void Main(string[] args)
  {
    string path = Directory.GetCurrentDirectory() + "//nlog.config";

    // Create an instance of Logger
    var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
    logger.Info("Program started");

    // Instantiate the database context
    var db = new DataContext();

    bool running = true;
    while (running)
    {
      Console.Clear();
      Console.WriteLine("Enter your selection:");
      Console.WriteLine("1) Display all blogs");
      Console.WriteLine("2) Add Blog");
      Console.WriteLine("3) Create Post");
      Console.WriteLine("4) Display Posts");
      Console.WriteLine("Enter q to quit");

      string choice = Console.ReadLine();

      switch (choice)
      {
        case "1":
          DisplayAllBlogs(db, logger);
          break;
        case "2":
          AddBlog(db, logger);
          break;
        case "3":
          CreatePost(db, logger);
          break;
        case "4":
          DisplayPosts(db, logger);
          break;
        case "q":
        case "Q":
          running = false;
          Console.WriteLine("Exiting...");
          break;
        default:
          Console.WriteLine("Invalid selection. Please try again.");
          break;
      }

      if (running)
      {
        Console.WriteLine("\nPress Enter to return to the menu...");
        Console.ReadLine();
      }
    }

    logger.Info("Program ended");
  }

  static void DisplayAllBlogs(DataContext db, Logger logger)
  {
    try
    {
      var query = db.Blogs.OrderBy(b => b.Name);
      Console.WriteLine("All blogs in the database:");
      foreach (var item in query)
      {
        Console.WriteLine(item.Name);
      }
      logger.Info("Displayed all blogs");
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Error displaying all blogs");
    }
  }

  static void AddBlog(DataContext db, Logger logger)
  {
    Console.Write("Enter a name for a new Blog: ");
    var name = Console.ReadLine();

    try
    {
      var blog = new Blog { Name = name };
      db.AddBlog(blog);
      db.SaveChanges();
      logger.Info("Blog added - {name}", name);
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Error adding blog");
    }
  }

  static void CreatePost(DataContext db, Logger logger)
  {
    try
    {
      // Display all available blogs to choose from
      var blogs = db.Blogs.OrderBy(b => b.Name).ToList();
      if (blogs.Count == 0)
      {
        Console.WriteLine("No blogs available. Please add a blog first.");
        return;
      }

      Console.WriteLine("Select a blog to post to:");
      foreach (var blog in blogs)
      {
        Console.WriteLine($"ID: {blog.BlogId}, Name: {blog.Name}");
      }

      // Prompt the user to enter a Blog ID
      Console.Write("Enter the Blog ID: ");
      if (int.TryParse(Console.ReadLine(), out int selectedBlogId) && blogs.Any(b => b.BlogId == selectedBlogId))
      {
        Console.Write("Enter the title of the post: ");
        var title = Console.ReadLine();
        Console.Write("Enter the content of the post: ");
        var content = Console.ReadLine();

        // Create and save the new post
        var post = new Post { Title = title, Content = content, BlogId = selectedBlogId };
        db.AddPost(post);
        db.SaveChanges();
        logger.Info("Post created - {title} for Blog ID {selectedBlogId}", title, selectedBlogId);
        Console.WriteLine("Post created successfully.");
      }
      else
      {
        Console.WriteLine("Invalid Blog ID.");
      }
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Error creating post");
    }
  }

  static void DisplayPosts(DataContext db, Logger logger)
  {
    try
    {
      Console.WriteLine("Would you like to:");
      Console.WriteLine("1) View posts from a specific blog");
      Console.WriteLine("2) View all posts");

      string choice = Console.ReadLine();

      if (choice == "1")
      {
        // Display all available blogs to choose from
        var blogs = db.Blogs.OrderBy(b => b.Name).ToList();
        if (!blogs.Any())
        {
          Console.WriteLine("No blogs available. Please add a blog first.");
          return;
        }

        Console.WriteLine("Select a blog to view posts from:");
        foreach (var blog in blogs)
        {
          Console.WriteLine($"ID: {blog.BlogId}, Name: {blog.Name}");
        }

        // Prompt the user to enter a Blog ID
        Console.Write("Enter the Blog ID: ");
        if (int.TryParse(Console.ReadLine(), out int selectedBlogId))
        {
          // Get the selected blog and its posts
          var selectedBlog = blogs.FirstOrDefault(b => b.BlogId == selectedBlogId);
          if (selectedBlog != null)
          {
            var posts = db.Posts
                          .Where(p => p.BlogId == selectedBlogId)
                          .OrderBy(p => p.Title)
                          .ToList();

            if (posts.Any())
            {
              Console.WriteLine($"Posts from blog '{selectedBlog.Name}':");
              foreach (var post in posts)
              {
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Content: {post.Content}");
                Console.WriteLine(new string('-', 40)); // Separator for readability
              }
            }
            else
            {
              Console.WriteLine("No posts found for this blog.");
            }
            logger.Info("Displayed posts from Blog ID {selectedBlogId}", selectedBlogId);
          }
          else
          {
            Console.WriteLine("Invalid Blog ID.");
          }
        }
        else
        {
          Console.WriteLine("Invalid input. Please enter a valid Blog ID.");
        }
      }
      else if (choice == "2")
      {
        // Display all posts, including the blog name
        var postsWithBlogs = db.Posts
            .Join(db.Blogs,
                  post => post.BlogId,
                  blog => blog.BlogId,
                  (post, blog) => new { BlogName = blog.Name, PostTitle = post.Title, PostContent = post.Content })
            .OrderBy(p => p.BlogName)
            .ThenBy(p => p.PostTitle)
            .ToList();

        if (postsWithBlogs.Any())
        {
          Console.WriteLine("All posts:");
          foreach (var post in postsWithBlogs)
          {
            Console.WriteLine($"Blog: {post.BlogName}");
            Console.WriteLine($"Title: {post.PostTitle}");
            Console.WriteLine($"Content: {post.PostContent}");
            Console.WriteLine(new string('-', 40)); // Separator for readability
          }
        }
        else
        {
          Console.WriteLine("No posts available.");
        }
        logger.Info("Displayed all posts with blog names");
      }
      else
      {
        Console.WriteLine("Invalid selection. Returning to menu.");
      }
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Error displaying posts");
    }
  }
}

