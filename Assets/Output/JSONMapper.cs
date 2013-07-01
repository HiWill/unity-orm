using System;

using System.Collections.Generic;
using UnityORM.Helper;
using System.Collections;

namespace UnityORM
{
	public class JSONMapper
	{
		
		public static readonly DateTime UnixTime = new DateTime(1970,1,1);
		
		ClassDescRepository Repository = ClassDescRepository.Instance;
		
		
		public JSONMapper ()
		{
		}
		
		
		public T[] Read<T>(string json){
			var jObjs = Json.Deserialize(json);
			return ReadFromJSONObject<T>(jObjs);
		}
		
		public T[] ReadFromJSONObject<T>(object jObjs){
			int size;
			
			if(jObjs is Dictionary<string,object>)
			{
				size = 1;
			}else if(jObjs is System.Collections.IList){
				size = (jObjs as System.Collections.IList).Count;
			}else if(jObjs.GetType().IsArray){
				size = (jObjs as object[]).Length;
			}else{
				throw new Exception(
					@"Wrong json object format.Must be List<Dictionary<stirng,object>> or Dictionary<string,object>.But was " + jObjs.GetType().Name);
			}
			
			T[] objects = ReflectionSupport.CreateNewInstances<T>(size);
			LoadFromJSONObject<T>(jObjs,objects,0,size);
			return objects;
		}
		
		public int Load<T>(string json, T[] objects){
			var jObjs = Json.Deserialize(json);
			return LoadFromJSONObject<T>(jObjs,objects,0,objects.Length);
		}
		
		public int LoadFromJSONObject<T>(object jsonObj, T[] objects,int offset,int size){
			if(size == 0) return 0;
			if(jsonObj is Dictionary<string,object>){
				var jObj = jsonObj as Dictionary<string,object>;
				var obj = objects[offset];
				LoadObj(jObj,obj);
				
				return 1;
			}else if(jsonObj.GetType().IsArray){
				var jobjs = jsonObj as object[];
				
				int readSize = Math.Min(size,jobjs.Length);
				if(size < 0){
					readSize = Math.Min(jobjs.Length,objects.Length - offset);
				}
				for(int i = 0;i < readSize;i++){
					var jobj = jobjs[i] as Dictionary<string,object>;
					if(jobj == null) throw new Exception("Wrong json object format.Must be List<Dictionary<stirng,object>>");
					LoadObj(jobj,objects[offset + i]);
					
				}
				return readSize;
				
			}else if(jsonObj is IList){
				var jobjs = jsonObj as IList;
				
				int readSize = Math.Min(size,jobjs.Count);
				if(size < 0){
					readSize = Math.Min(jobjs.Count,objects.Length - offset);
				}
				for(int i = 0;i < readSize;i++){
					var jobj = jobjs[i] as Dictionary<string,object>;
					if(jobj == null) throw new Exception("Wrong json object format.Must be List<Dictionary<stirng,object>>");
					LoadObj(jobj,objects[offset + i]);
					
				}
				return readSize;
			}else{
				throw new Exception("Wrong json object format.Must be List<Dictionary<stirng,object>> or Dictionary<string,object>");
			}
		}
				
		void LoadObj<T>(Dictionary<string,object> jObj,T target){
			var classDesc = Repository.GetClassDesc<T>();
			foreach(var field in classDesc.FieldDescs){
				if(jObj.ContainsKey(field.NameInJSON)){
					field.SetValue(target,jObj[field.NameInJSON]);
				}
			}
		}
		
		public object ToJsonObject<T>(T[] objects){
			var classDesc = Repository.GetClassDesc<T>();
			
			List<Dictionary<string,object>> jsonObjs = new List<Dictionary<string, object>>();
			
			foreach(T obj in objects){
				Dictionary<string,object> dict = new Dictionary<string, object>();
				
				foreach(var field in classDesc.FieldDescs){
					dict.Add(field.NameInJSON,WriteCastIfNeeded(field.GetValue(obj)));
				}
				jsonObjs.Add(dict);
			}
			return jsonObjs;
		}
		
		public string Write<T>(T[] objects){
			
			object jsonObjs = ToJsonObject<T>(objects);
			
			return Json.Serialize(jsonObjs);
		}
		
		object WriteCastIfNeeded(object v){
			if(v is DateTime){
				return (((DateTime)v) - UnixTime).TotalSeconds;
			}else{
				return v;
			}
		}
	}
}

