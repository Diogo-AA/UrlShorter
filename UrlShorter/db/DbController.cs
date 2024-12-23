using Microsoft.Data.Sqlite;
using UrlShorter.Models;

namespace UrlShorter.Db;

public class DbController
{
    private readonly SqliteConnection _connection;

    public DbController()
    {
        _connection = new SqliteConnection("Data Source=data/data.db");
        _connection.Open();
        CreateDbIfNotExists();
    }

    ~DbController()
    {
        _connection.Close();
    }

    private void CreateDbIfNotExists()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS user (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                username TEXT UNIQUE
            );
            CREATE TABLE IF NOT EXISTS urls (
                user_id INTEGER NOT NULL,
                shorted_url TEXT NOT NULL PRIMARY KEY,
                original_url TEXT NOT NULL,
                FOREIGN KEY(user_id) REFERENCES user(id)
            );
        ";
        command.ExecuteNonQuery();
    }

#region User funcions
    public User? GetUser(long userId)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT username
            FROM USER
            WHERE id = $userId
        ";
        command.Parameters.AddWithValue("$userId", userId);
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
        
        if (!reader.HasRows)
            return null;
        
        reader.Read();
        var user = new User()
        {
            Id = userId,
            Username = reader.GetString(0)
        };

        return user;
    }

    public User? GetUser(string username)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT id
            FROM USER
            WHERE username = $username
        ";
        command.Parameters.AddWithValue("$username", username);
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
        
        if (!reader.HasRows)
            return null;
        
        reader.Read();
        var user = new User()
        {
            Id = reader.GetInt64(0),
            Username = username
        };

        return user;
    }

    public List<User> GetAllUsers()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT id, username
            FROM USER
        ";
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult);
        
        if (!reader.HasRows)
            return [];
        
        var users = new List<User>();
        while (reader.Read())
        {
            var user = new User()
            {
                Id = reader.GetInt64(0),
                Username = reader.GetString(1)
            };
            users.Add(user);
        }

        return users;
    }

    public User? AddUser(string username)
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO user (username)
                VALUES ($username);
                SELECT last_insert_rowid();
            ";
            command.Parameters.AddWithValue("$username", username);
            var userId = command.ExecuteScalar();

            if (userId == null)
                return null;

            return new User()
            {
                Id = (long)userId,
                Username = username
            };
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_CONSTRAINT)
        {
            return null;
        }
    }

    public bool DeleteUser(long id)
    {
        try
        {
            var command = _connection.CreateCommand();
            command.CommandText = @"

                DELETE FROM user
                WHERE id = $id
            ";
            command.Parameters.AddWithValue("$id", id);
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected == 1;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_NOTFOUND)
        {
            return false;
        }
    }
#endregion

#region Url functions
    public Url? GetUrl(string shortedUrl)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT userId, originalUrl
            FROM URL
            WHERE shortedUrl = $shortedUrl
        ";
        command.Parameters.AddWithValue("$shortedUrl", shortedUrl);
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow);
        
        if (!reader.HasRows)
            return null;
        
        reader.Read();
        var url = new Url()
        {
            UserId = reader.GetInt64(0),
            ShortedUrl = shortedUrl,
            OriginalUrl = reader.GetString(1)
        };

        return url;
    }

    public List<Url> GetAllUrlsFromUser(long userId)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT shortedUrl, originalUrl
            FROM URL
            WHERE userId = $userId
        ";
        command.Parameters.AddWithValue("$userId", userId);
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult);
        
        if (!reader.HasRows)
            return [];

        var urls = new List<Url>();        
        while (reader.Read())
        {
            var url = new Url()
            {
                UserId = userId,
                ShortedUrl = reader.GetString(0),
                OriginalUrl = reader.GetString(1)
            };
            urls.Add(url);
        }

        return urls;
    }

    public List<Url> GetAllUrlsFromUser(string username)
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT url.userId, shortedUrl, originalUrl
            FROM url
            JOIN user on url.userId = user.id
            WHERE user.username = $username
        ";
        command.Parameters.AddWithValue("$username", username);
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult);
        
        if (!reader.HasRows)
            return [];

        var urls = new List<Url>();        
        while (reader.Read())
        {
            var url = new Url()
            {
                UserId = reader.GetInt64(0),
                ShortedUrl = reader.GetString(1),
                OriginalUrl = reader.GetString(2)
            };
            urls.Add(url);
        }

        return urls;
    }

    public List<Url> GetAllUrls()
    {
        var command = _connection.CreateCommand();
        command.CommandText = @"
            SELECT userId, shortedUrl, originalUrl
            FROM URL
        ";
        using var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleResult);
        
        if (!reader.HasRows)
            return [];

        var urls = new List<Url>();        
        while (reader.Read())
        {
            var url = new Url()
            {
                UserId = reader.GetInt64(0),
                ShortedUrl = reader.GetString(1),
                OriginalUrl = reader.GetString(2)
            };
            urls.Add(url);
        }

        return urls;
    }
#endregion
}