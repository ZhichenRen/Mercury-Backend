﻿using System;
using System.Collections.Generic;
using Mercury_Backend.Models;

namespace Mercury_Backend.Utils
{
    public class Simplify
    {
        public static SimplifiedOrder SimplifyOrder(Order order)
        {
            var simplifiedOrder = new SimplifiedOrder(order.Id, order.Commodity.Name, order.BuyerId, order.Commodity.OwnerId, (decimal)order.Commodity.Price
                , (int)order.Count, order.Status, order.Commodity.Cover);
            return simplifiedOrder;
        }

        public static SimplifiedCommodity SimplifyCommodity(Commodity commodity)
        {
            var commodityTag = new List<string>();
            foreach (var t in commodity.CommodityTags)
            {
                commodityTag.Add(t.Tag);
            }

            // var comment = new List<SimplifiedComment>();
            
            var tag = commodityTag;
            var simplifiedCommodity = new SimplifiedCommodity(
                commodity.Id,
                commodity.Name,
                commodity.Price == null ? 0: (decimal)commodity.Price,
                commodity.Likes == null?0: (int)commodity.Likes ,
                commodity.Cover,
                commodity.OwnerId,
                commodity.Owner.Nickname,
                commodity.Owner.Avatar.Path,
                commodityTag,
                commodity.Classification,
                commodity.Clicks == null ? 0: commodity.Clicks,
                commodity.Stock,
                commodity.Video == null? "": commodity.Video.Path,
                commodity.Condition == null ? "good": commodity.Condition ,
                commodity.ForRent == null ? false: commodity.ForRent,
                commodity.Description
            );

            return simplifiedCommodity;
        }

        public static SimplifiedPost SimplfyPost(NeedPost post)
        {
            var simplifiedPost = new SimplifiedPost(post.Id, post.Title, post.Sender.Nickname, post.Content, post.SenderId,
                post.Sender.Avatar.Path);
            return simplifiedPost;
        }

        public static SimplifiedUser SimplifyUser(SchoolUser user)
        {
            var simplifiedUser = new SimplifiedUser(user.SchoolId, user.Nickname,
                user.Avatar.Path, user.RealName, user.Role);
            return simplifiedUser;
        }

        public static SimplifiedComment SimplifyComment(PostComment comment)
        {
            var simplifiedComment = new SimplifiedComment(comment.Id, comment.SenderId, comment.Sender.Nickname,
                comment.Sender.Avatar.Path, (DateTime)comment.Time, comment.Content);
            return simplifiedComment;
        }

        public static SimplifiedRating SimplifyRating(Rating rating)
        {
            var simplifiedRating = new SimplifiedRating(
                rating.User.Nickname,
                rating.User.Avatar.Path,
                rating.UserId,
                rating.Comment,
                rating.Rate
            );
            return simplifiedRating;
        }
    }
}