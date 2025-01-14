﻿using Mercury_Backend.Contexts;
using Mercury_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Linq;

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mercury_Backend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Mercury_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase
    {
        private readonly ModelContext context;
        public CommodityController(ModelContext modelContext)
        {
            context = modelContext;
        }

        // GET api/<CommodityController>
        [HttpGet]
        public string Get(string id, string classification, string keyword, string ownerName, string userId, string tag)
        {
            Console.WriteLine("id:" + id);
            Console.WriteLine("classification:" + classification);
            var flag = 0;
            JObject msg = new JObject();
            var commodityList = new List<Commodity>();
            if (id == null && classification == null && keyword == null && ownerName == null && userId == null && tag == null)
            {
                flag = 1;
                try
                {
                    var judge = Request.Form["keyword"].ToString();
                }
                catch // No constraints.
                {
                    try
                    {
                        var simplifiedList = new List<SimplifiedCommodity>();
                        var tmpList = context.Commodities.Where(s => true).Include(commodity => commodity.CommodityTags)
                                .Include(commodity => commodity.Owner).ThenInclude(owner => owner.Avatar)
                            ;
                        
                        commodityList = tmpList.ToList<Commodity>();
                        
                        for (int i = 0; i < commodityList.Count; i++)
                        {
                        
                            simplifiedList.Add(Simplify.SimplifyCommodity(commodityList[i]));
                        }
                        
                        msg["commodityList"] = JToken.FromObject(simplifiedList, new JsonSerializer()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore //忽略循环引用，默认是throw exception
                        });
                        var idList = commodityList.Select(s => s.Id).ToList();
                        var tags = new List<CommodityTag>();
                        for (int i = 0; i < idList.Count; i++)
                        {
                            var tmpTag = context.CommodityTags.Where(tag => tag.CommodityId == idList[i])
                                .ToList();

                            tags = tags.Concat(tmpTag).ToList();
                        
                        }
                
                        var tagSet = tags.Select(s => s.Tag).ToList();
                        tagSet = tagSet.Distinct().ToList();
                        msg["tags"] = JToken.FromObject(tagSet);
                    
                        

                        msg["Code"] = "200";


                        msg["totalPage"] = commodityList.Count;
                    }
                    catch (Exception e1)
                    {
                        Console.WriteLine(e1);

                        msg["Code"] = "403";
                        msg["Description"] = "Error occured when getting data from model.";

                    }
                    return JsonConvert.SerializeObject(msg);
                
                }
                
            }

            var strKeyWord = "";
            var strId = "";
            var intClass = -1;
            var strOwnerName = "";
            var strUserId = "";
            var strTag = "";
            if (flag == 1) // No query but exist form
            {
                strKeyWord = Request.Form["keyword"].ToString();
                strId = Request.Form["id"].ToString();
                intClass = Request.Form["classification"].ToString() == ""? -1 : Byte.Parse(Request.Form["classification"]);
                strOwnerName = Request.Form["ownerName"].ToString();
                strUserId = Request.Form["userId"].ToString();
                strTag = Request.Form["tag"].ToString();
            }
            else
            {
                strKeyWord = keyword;
                strId = id;
                intClass = classification == null ? -1 : Byte.Parse(classification);
                strOwnerName = ownerName;
                strUserId = userId;
                strTag = tag;
            }
            

            if (strKeyWord != null && strKeyWord != "") {
                var tmpList = context.Commodities.Where(b => b.Name.Contains(strKeyWord)).
                    Include(commodity => commodity.Video).
                    Include(commodity => commodity.CommodityTags).
                    Include(commodity => commodity.Owner).ThenInclude(owner => owner.Avatar).ToList<Commodity>();
                
                commodityList = tmpList;
            }
            
            
            else if (strOwnerName != null && strOwnerName != "")
            {
                
                
                // msg["commodityList"] = JToken.FromObject(commodityList);
                
                var usrs = context.SchoolUsers.Where(b => b.Nickname.Contains(strOwnerName)).ToList();
                var idList = usrs.Select(s => new {s.SchoolId}).ToList();
                
                for (int i = 0; i < idList.Count; i++)
                {
                    var tmpList = context.Commodities.Where(b => b.OwnerId== idList[i].SchoolId).
                       Include(commodity => commodity.Video).
                        Include(commodity => commodity.CommodityTags).Include(commodity => commodity.Owner).ThenInclude(owner => owner.Avatar).ToList<Commodity>();
                    commodityList = commodityList.Concat(tmpList).ToList<Commodity>();
                }
            }
            // 按类别搜索
            else if (intClass != -1)
            {
                
               commodityList = context.Commodities.Where(b => b.Classification == intClass).
                   Include(commodity => commodity.Video).
                   Include(commodity => commodity.CommodityTags).
                   Include(commodity => commodity.Owner).
                   ThenInclude(owner => owner.Avatar).ToList<Commodity>();
            }
            // 按学号搜索
            else if (strUserId != null && strUserId != "")
            {
                
                // msg["commodityList"] = JToken.FromObject(commodityList);
                
                var usrs = context.SchoolUsers.
                    Where(b => b.SchoolId == strUserId).ToList();
                var idList = new List<string>();
                idList.Add(strUserId);
                for (int i = 0; i < idList.Count; i++)
                {
                    var tmpList = context.Commodities.Where(b => b.OwnerId == idList[i]);
                    var tmpList1 = tmpList.Include(commodity => commodity.CommodityTags).Include(commodity => commodity.Video);
                    var tmpList2 = tmpList1.Include(commodity => commodity.Owner);
                    var tmpList3 = tmpList2.ThenInclude(owner => owner.Avatar).ToList<Commodity>();
                    
                    commodityList = commodityList.Concat(tmpList3).ToList<Commodity>();
                }
                
            }
            // 按标签搜索
            else if (strTag != "" && strTag != null)
            {
                
                var tagList = context.CommodityTags.Where(b => b.Tag == strTag);
                var idList = tagList.Select(s => new {s.CommodityId}).ToList();
                for (int i = 0; i < idList.Count; i++)
                {
                    var tmpList = context.Commodities.Where(b => idList[i].CommodityId == b.Id).Include(commodity => commodity.CommodityTags).Include(commodity => commodity.Owner).ThenInclude(owner => owner.Avatar).ToList<Commodity>();
                    commodityList = commodityList.Concat(tmpList).ToList<Commodity>();
                }
            }
            
            
            else if (strId != "" && strId != null) 
            {
                try
                {
                    
                    commodityList = context.Commodities.Where(s=>s.Id == strId).
                        Include(commodity => commodity.CommodityTags).
                        Include(commodity => commodity.Owner).
                        ThenInclude(owner => owner.Avatar).
                        Include(c => c.Video).ToList<Commodity>();;
                    if (commodityList.Count == 0)
                    {
                        msg["Code"] = "404";
                        msg["Description"] = "No such id";
                        return JsonConvert.SerializeObject(msg);
                    }
                    // commodityList.Add(targetComm);
                    var targetComm = commodityList[0];
                    if (commodityList[0].Clicks == null)
                    {
                        commodityList[0].Clicks = 1;
                    }
                    else
                    {
                        commodityList[0].Clicks++;
                    }

                    if (userId != null && userId != "")
                    {
                        var vw = new View();
                        vw.Time = DateTime.Now;
                        vw.Commodity = targetComm;
                        vw.CommodityId = targetComm.Id;
                        vw.User = context.SchoolUsers.Find(userId);
                        vw.UserId = vw.User.SchoolId;
                        targetComm.Views.Add(vw);
                    }

                    var orderList = context.Orders.Where(s => s.CommodityId == strId).ToList()
                        .Select(s => new {s.Id}).ToList();
                    var commentList = new List<Rating>();
                    foreach (var t in orderList)
                    {
                        commentList = commentList.Concat(context.Ratings.
                            Where(s => s.OrderId == t.Id).
                            Include(c => c.User)
                            .ThenInclude(c => c.Avatar)
                            .ToList()).ToList();
                        // commentList.Add(Simplify.SimplifyComment()));
                    }
                    
                    var simplifiedOrderList = new List<SimplifiedRating>();
                    foreach (var t in commentList)
                    {
                        simplifiedOrderList.Add(Simplify.SimplifyRating(t));
                    }

                    var mediaList = context.CommodityImages.Where(s => s.CommodityId == strId).ToList().Select(s => s.ImageId).ToList();
                    var imgList = new List<string>();
                    for (int i = 0; i < mediaList.Count; i++)
                    {
                        imgList.Add(context.Media.Find(mediaList[i]).Path);
                    }

                    msg["ImgList"] = JToken.FromObject(imgList, new JsonSerializer()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore //忽略循环引用，默认是throw exception
                    });
                    msg["Comments"] = JToken.FromObject(simplifiedOrderList, new JsonSerializer()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore //忽略循环引用，默认是throw exception
                    });
                    try
                    {
                        context.SaveChanges();
                    }
                    catch
                    {
                        msg["Code"] = "403";
                        msg["Description"] = "Error occured when changing data from model.";
                    }
                }
                catch
                {
                    msg["Code"] = "403";
                    msg["Description"] = "Error occured when getting data from model.";
                }
            }
            else
            {

                msg["Code"] = "400";
                msg["Description"] = "You have not submitted any effective form data.";

                return JsonConvert.SerializeObject(msg);
            }
            try
            {
                var simplifiedList = new List<SimplifiedCommodity>();
                
                for (int i = 0; i < commodityList.Count; i++)
                {
                    try
                    {
                        simplifiedList.Add(Simplify.SimplifyCommodity(commodityList[i]));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(commodityList[i]);
                        Console.WriteLine(e);
                    }

                }
                msg["commodityList"] = JToken.FromObject(simplifiedList, new JsonSerializer()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore //忽略循环引用，默认是throw exception
                });
                

                var idList = commodityList.Select(s => s.Id).ToList();
                var tags = new List<CommodityTag>();
                for (int i = 0; i < idList.Count; i++)
                {
                    var tmpTag = context.CommodityTags.
                        Where(tag => tag.CommodityId == idList[i])
                        .ToList();
                   
                    tags = tags.Concat(tmpTag).ToList();
                }
                
                var tagSet = tags.Select(s => s.Tag).ToList();
                
                
                tagSet = tagSet.Distinct().ToList();
                msg["tags"] = JToken.FromObject(tagSet);
                var classSet = commodityList.Select(s => s.Classification).ToList().Distinct().ToList();
                msg["classifications"] = JToken.FromObject(classSet);
                msg["Code"] = "200";
                msg["totalPage"] = commodityList.Count;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                msg["Code"] = "403";
                msg["Description"] = "Error occured when getting data from model.";

            }
            return JsonConvert.SerializeObject(msg);
        }
        
        

        // POST api/<CommodityController>
        [HttpPost]
        public async Task<string> Post([FromForm]Commodity newCommodity, [FromForm] List<IFormFile> files, [FromForm] List<string> tags)
        {
            JObject msg = new JObject();
            
            var id = Generator.GenerateId(12);
            newCommodity.Id = id;
            newCommodity.Likes = 0;
            newCommodity.Clicks = 0;
            newCommodity.Popularity = 0;
            Console.WriteLine(newCommodity.Price);
            var pathList = new List<string>();
            try
            {
                Console.WriteLine(files.Count());
                if (files.Any())
                {
                    Console.WriteLine("No files uploaded.");
                }

                var flag = 0;
                for (int i = 0; i < files.Count(); i++)
                {
                    var tmpVideoId = Generator.GenerateId(20);
                    var splitFileName = files[i].FileName.Split('.');
                    var len = splitFileName.Length;
                    var postFix = splitFileName[len - 1];
                    var path = "";
                    if (postFix == "jpg" || postFix == "jpeg" || postFix == "gif" || postFix == "png")
                    {
                        path = "Media" + "/Image/" + tmpVideoId + '.' + postFix;
                        
                        if (Directory.Exists(path))
                        {
                            Console.WriteLine("This path exists.");
                        }
                        else
                        {
                            Directory.CreateDirectory("Media");
                            Directory.CreateDirectory("Media/Image");
                        }

                        var med = new Medium();
                        med.Id = tmpVideoId;
                        med.Type = "Image";
                        med.Path = path;
                        context.Media.Add(med);
                        var comImg = new CommodityImage
                        {
                            Commodity = newCommodity,
                            CommodityId = newCommodity.Id,
                            Image = med,
                            ImageId = med.Id
                        };
                        if (flag == 0)
                        {
                            newCommodity.Cover = path;
                            flag = 1;
                        }
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await files[i].CopyToAsync(stream);
                        }
                        // imageStream.Save(path);
                        newCommodity.CommodityImages.Add(comImg);
                    }
                    else if (postFix == "mov" || postFix == "mp4" || postFix == "wmv" || postFix == "rmvb" || postFix == "3gp")
                    {
                        path = "Media" + "/Video/" + tmpVideoId + '.' + postFix;
                        Console.WriteLine(path);
                        pathList.Add(path);
                        if (Directory.Exists(path))
                        {
                            Console.WriteLine("This path exists.");
                        }
                        else
                        {
                            Directory.CreateDirectory("Media");
                            Directory.CreateDirectory("Media/Video");
                        }
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await files[i].CopyToAsync(stream);
                        }

                        var video = new Medium();
                        video.Id = tmpVideoId;
                        video.Type = "Video";
                        video.Path = path;
                        context.Media.Add(video);
                        newCommodity.VideoId = tmpVideoId;
                    }
                    else
                    {
                        Console.WriteLine("Not a media file.");
                    }

                    if (flag == 0)
                    {
                        newCommodity.Cover = "Media/Image/Default.png";
                    }
                }

                foreach (var t in tags)
                {
                    var tmpTag = new CommodityTag();
                    tmpTag.CommodityId = id;
                    tmpTag.Tag = t;
                    context.CommodityTags.Add(tmpTag);
                }
                context.Commodities.Add(newCommodity);
                // Console.WriteLine("haha")
                
                
                context.SaveChanges();

                msg["CommodityId"] = id;
                msg["Code"] = "201";
            }
            catch (Exception e)
            {
                msg["Code"] = "403";
                msg["Description"] = "Internal error occured when posting";
                Console.WriteLine(e.ToString());
            }
            return JsonConvert.SerializeObject(msg);
        }

        // PUT api/<CommodityController>/5
        [HttpPut("{id}")]
        public string Put(string id)
        {
            JObject msg = new JObject();

            msg["Code"] = "200";

            try
            {
                var test = Request.Form["test"].ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                msg["Code"] = "400";
                msg["Description"] = "You have not submitted any form data.";

                
                return JsonConvert.SerializeObject(msg);
            }
            var commodityToChange = context.Commodities.Find(id);
            if (commodityToChange == null)
            {

                msg["Code"] = "404";
                msg["Description"] = "The ID does not exist in database.";

                 return JsonConvert.SerializeObject(msg);
            }
            var detailMsg = "Integrity constraint invoked by";
            
            
           
            
            if (Request.Form["id"].ToString() != "")
            {
                // commodityToChange.OwnerId = Request.Form["owner_id"].ToString();
                // context.SaveChanges();

                msg["Code"] = "403";

                detailMsg += " id ";
            }   
            if (Request.Form["owner_id"].ToString() != "")
            {
                detailMsg += " owner_id ";
                
            }

            if (Request.Form["video_id"].ToString() != "")
            {
                // commodityToChange.VideoId = Request.Form["video_id"].ToString();

                msg["Code"] = "403";

                detailMsg += " video_id ";
                
            }
            if (Request.Form["condition"].ToString() != "")
            {
                commodityToChange.Condition = Request.Form["condition"].ToString();
                detailMsg += " owner_id ";
                
            }

            if (Request.Form["price"].ToString() != "")
            {
                try
                {
                    commodityToChange.Price = Decimal.ToInt32(int.Parse(Request.Form["price"].ToString()));
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " price ";
                }
            }
            
            if (Request.Form["stock"].ToString() != "")
            {
                try
                {
                    commodityToChange.Stock = Decimal.ToByte(int.Parse(Request.Form["stock"].ToString()));
                }
                catch
                {

                    msg["Code"] = "403";

                    detailMsg += " stock ";
                }
            }
            
            if (Request.Form["forRent"].ToString() != "")
            {
                try
                {
                    commodityToChange.ForRent = Request.Form["forRent"].ToString() != "0";
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " forRent ";
                }
            }
            if (Request.Form["clicks"].ToString() != "")
            {
                try
                {
                    commodityToChange.Clicks = int.Parse(Request.Form["clicks"].ToString());
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " clicks ";
                }
            }
            
            if (Request.Form["likes"].ToString() != "")
            {
                try
                {
                    commodityToChange.Likes = int.Parse(Request.Form["likes"].ToString());
                }
                catch 
                {
                    msg["Code"] = "403";
                    detailMsg += " likes ";
                }
            }
            if (Request.Form["popularity"].ToString() != "")
            {
                try
                {
                    commodityToChange.Popularity = byte.Parse(Request.Form["popularity"].ToString());
                }
                catch 
                {

                    msg["Code"] = "403";


                    detailMsg += " popularity ";
                }
            }
            
            if (Request.Form["popularity"].ToString() != "")
            {
                try
                {
                    commodityToChange.Likes = int.Parse(Request.Form["popularity"].ToString());
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " popularity ";
                }
            }
            if (Request.Form["classification"].ToString() != "")
            {
                msg["Code"] = "403";

                detailMsg += " classification ";
                
            }
            if (Request.Form["unit"].ToString() != "")
            {
                try
                {
                    commodityToChange.Unit = Request.Form["unit"].ToString();
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " unit ";
                }
            }
            if (Request.Form["name"].ToString() != "")
            {
                try
                {
                    commodityToChange.Name = Request.Form["name"].ToString();
                }
                catch 
                {

                    msg["Code"] = "403";

                    detailMsg += " name ";
                }
            }


            if (msg["Code"].ToString() == "200")
            {
                context.SaveChanges();
                msg["changedCommodity"] = JToken.FromObject(commodityToChange);
            }

            if (detailMsg == "Integrity constraint invoked by")
            {
                detailMsg += " nothing.";
            }
            else
            {
                msg["Description"] = detailMsg;
                // detailMsg += ".";
            }

            // msg["detailMessage"] = detailMsg;
            return JsonConvert.SerializeObject(msg);
        }

        // DELETE api/<CommodityController>/5
        [HttpDelete("{id}")]
        public string Delete(string id)
        {
            JObject msg = new JObject();
            try
            {
                var commodityToDelete = context.Commodities.Where(c => c.Id == id)
                    .Include(c => c.CommodityImages)
                    .ThenInclude(ci => ci.Image)
                    .Include(c => c.Orders).Single();
                if (commodityToDelete == null)
                {

                    msg["Code"] = "404";
                    msg["Description"] = "No such id.";

                    return JsonConvert.SerializeObject(msg);
                }
                if (commodityToDelete.Orders.Count != 0)
                {
                    msg["Code"] = "403";
                    msg["Description"] = "Cannot delete a commodity which appears in an order";

                    return JsonConvert.SerializeObject(msg);
                }
                foreach (var ci in commodityToDelete.CommodityImages)
                {
                    if(System.IO.File.Exists(ci.Image.Path) && ci.Image.Path != "Media/Image/Default.png")
                    {
                        Console.WriteLine(ci.Image.Path);
                        try
                        {
                            System.IO.File.Delete(ci.Image.Path);
                        }
                        catch (System.IO.IOException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
                context.Commodities.Remove(commodityToDelete);
                context.SaveChanges();
                msg["Code"] = "200";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                msg["Code"] = "403";
                msg["Description"] = "Error occured when putting data into model.";
            }
            return JsonConvert.SerializeObject(msg);
        }
    }
}
