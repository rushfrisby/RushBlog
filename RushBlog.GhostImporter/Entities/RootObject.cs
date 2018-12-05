using System.Collections.Generic;

namespace RushBlog.GhostImporter.Entities
{
	public class Meta
	{
		public long exported_on { get; set; }
		public string version { get; set; }
	}

	public class Post
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string title { get; set; }
		public string slug { get; set; }
		public string markdown { get; set; }
		public string html { get; set; }
		public string image { get; set; }
		public int featured { get; set; }
		public int page { get; set; }
		public string status { get; set; }
		public string language { get; set; }
		public string meta_title { get; set; }
		public object meta_description { get; set; }
		public int author_id { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
		public string published_at { get; set; }
		public int? published_by { get; set; }
		public string visibility { get; set; }
		public object mobiledoc { get; set; }
		public object amp { get; set; }
	}

	public class User
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string name { get; set; }
		public string slug { get; set; }
		public string password { get; set; }
		public string email { get; set; }
		public string image { get; set; }
		public object cover { get; set; }
		public string bio { get; set; }
		public string website { get; set; }
		public string location { get; set; }
		public object accessibility { get; set; }
		public string status { get; set; }
		public string language { get; set; }
		public object meta_title { get; set; }
		public object meta_description { get; set; }
		public object tour { get; set; }
		public string last_login { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
		public string visibility { get; set; }
		public object facebook { get; set; }
		public object twitter { get; set; }
	}

	public class Role
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
	}

	public class RolesUser
	{
		public int id { get; set; }
		public int role_id { get; set; }
		public int user_id { get; set; }
	}

	public class Permission
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string name { get; set; }
		public string object_type { get; set; }
		public string action_type { get; set; }
		public object object_id { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
	}

	public class PermissionsRole
	{
		public int id { get; set; }
		public int role_id { get; set; }
		public int permission_id { get; set; }
	}

	public class Setting
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string key { get; set; }
		public string value { get; set; }
		public string type { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
	}

	public class PostsTag
	{
		public int id { get; set; }
		public int post_id { get; set; }
		public int tag_id { get; set; }
		public int sort_order { get; set; }
	}

	public class Tag
	{
		public int id { get; set; }
		public string uuid { get; set; }
		public string name { get; set; }
		public string slug { get; set; }
		public string description { get; set; }
		public object image { get; set; }
		public object parent_id { get; set; }
		public object meta_title { get; set; }
		public object meta_description { get; set; }
		public string created_at { get; set; }
		public int created_by { get; set; }
		public string updated_at { get; set; }
		public int updated_by { get; set; }
		public string visibility { get; set; }
	}

	public class Data
	{
		public List<Post> posts { get; set; }
		public List<User> users { get; set; }
		public List<Role> roles { get; set; }
		public List<RolesUser> roles_users { get; set; }
		public List<Permission> permissions { get; set; }
		public List<object> permissions_users { get; set; }
		public List<PermissionsRole> permissions_roles { get; set; }
		public List<object> permissions_apps { get; set; }
		public List<Setting> settings { get; set; }
		public List<PostsTag> posts_tags { get; set; }
		public List<object> apps { get; set; }
		public List<object> app_settings { get; set; }
		public List<object> app_fields { get; set; }
		public List<Tag> tags { get; set; }
		public List<object> subscribers { get; set; }
	}

	public class Db
	{
		public Meta meta { get; set; }
		public Data data { get; set; }
	}

	public class RootObject
	{
		public List<Db> db { get; set; }
	}
}
