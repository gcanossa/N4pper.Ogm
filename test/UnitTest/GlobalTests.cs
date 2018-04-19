﻿//using AsIKnow.XUnitExtensions;
//using N4pper;
//using N4pper.Ogm;
//using N4pper.Ogm.Entities;
//using Neo4j.Driver.V1;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnitTest.TestModel;
//using Xunit;

//namespace UnitTest
//{
//    [TestCaseOrderer(AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeName, AsIKnow.XUnitExtensions.Constants.PriorityOrdererTypeAssemblyName)]
//    [Collection(nameof(Neo4jCollection))]
//    public class GlobalTests
//    {
//        protected Neo4jFixture Fixture { get; set; }

//        public GlobalTests(Neo4jFixture fixture)
//        {
//            Fixture = fixture;
//        }

//        private GraphContext SetUp()
//        {
//            return 
//                Fixture.GetService<Neo4jFixture.GlobalTestContext>()
//                ;
//        }

//        private int GetEntityNodesCount(ISession session)
//        {
//            return session.Run($"MATCH (p) WHERE NOT p:{N4pper.Constants.GlobalIdentityNodeLabel} RETURN COUNT(p)").Select(x => x.Values[x.Keys[0]].As<int>()).First();
//        }

//        private void TestBody(Action<ISession, GraphContext> body)
//        {
//            GraphContext ctx = SetUp();

//            using (ISession session = ctx.Driver.Session())
//            {
//                int count = GetEntityNodesCount(session);
//                try
//                {
//                    body(session, ctx);
//                }
//                finally
//                {
//                    Assert.Equal(count, GetEntityNodesCount(session));
//                }
//            }
//        }

//        [TestPriority(0)]
//        [Trait("Category", nameof(GlobalTests))]
//        [Fact(DisplayName = nameof(NodeCreation))]
//        public void NodeCreation()
//        {
//            TestBody((session, ctx)=> 
//            {
//                var book = session.AddOrUpdateNode(new Book { Name = "Dune", Index=0 });
//                var chapter1 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 1", Index = 0 });
//                var chapter2 = session.AddOrUpdateNode(new Chapter { Name = "Capitolo 2", Index = 1 });

//                session.LinkNodes<Links.RelatesTo, Book, Chapter>(book, chapter1);
//                session.LinkNodes<Links.RelatesTo, Book, Chapter>(book, chapter2);

//                IEnumerable<Book> tmp = session
//                .ExecuteQuery<Book, IEnumerable<Chapter>>(
//                    p=> $"match {p.Node<Book>(p.Symbol("p"))._(p.Rel<Links.RelatesTo>(p.Symbol()))._V(p.Node<Chapter>(p.Symbol("q")))} return p, collect(q)",
//                    (b, c)=>
//                    {
//                        b.Chapters = new List<Chapter>();

//                        b.Chapters.AddRange(c);
//                        foreach (Chapter item in c)
//                        {
//                            item.Book = b;
//                        }

//                        return b;
//                    });

//                Assert.Equal(1, tmp.Count());
//                Assert.Equal(2, tmp.First().Chapters.Count());
//                Assert.Equal(tmp.First().Id, tmp.First().Chapters.First().Book.Id);

//                session.DeleteNode(chapter2);
//                session.DeleteNode(chapter1);
//                session.DeleteNode(book);
//            });
//        }

//        [TestPriority(0)]
//        [Trait("Category", nameof(GlobalTests))]
//        [Fact(DisplayName = nameof(ManagedCreation))]
//        public void ManagedCreation()
//        {
//            TestBody((session, ctx) =>
//            {
//                var user = new User() { Birthday = new DateTime(1988, 1, 30), Name = "Gianmaria" };
//                var user2 = new User() { Birthday = new DateTime(1989, 9, 28), Name = "Valentina" };
//                var book = new Book { Name = "Dune", Index = 0, Owner = user, Contributors = new List<User>() { user, user2 } };
//                var chapter1 = new Chapter { Name = "Capitolo 1", Index = 0, Book = book, Owner = user, Contributors = new List<User>() { user, user2 } };
//                var chapter2 = new Chapter { Name = "Capitolo 2", Index = 1, Book = book, Owner = user, Contributors = new List<User>() { user, user2 } };
//                book.Chapters = new List<Chapter>() {chapter1, chapter2 };

//                ctx.Add(user);
//                ctx.Add(chapter2);

//                ctx.SaveChanges();

//                //var chapter3 = new Chapter { Name = "Capitolo 3", Index = 1, Book = book };
//                //book.Chapters = new List<Chapter>() { chapter1, chapter3 };

//                //ctx.SaveChanges(session);

//                Book bookQ = ctx.Query<Book>(k=> 
//                {
//                    k.Include(p => p.Chapters).Include(p => p.Contributors);
//                    k.Include(p => p.Chapters).Include(p => p.Owner);
//                    k.Include(p => p.Contributors);
//                    k.Include(p => p.Owner);
//                }).First(p=>p.Id>0);
                
//                Assert.Equal(2, bookQ.Chapters.Count());
//                Assert.Equal(bookQ.Id, bookQ.Chapters.First().Book.Id);
//                Assert.Equal(bookQ.Contributors.First().Id, bookQ.Chapters.First().Contributors.First().Id);
//                Assert.Equal(bookQ, bookQ.Chapters.First().Book);

//                //ctx.Remove(chapter3);
//                ctx.Remove(user);
//                ctx.Remove(user2);
//                ctx.Remove(chapter2);
//                ctx.Remove(chapter1);
//                ctx.Remove(book);

//                ctx.SaveChanges();
//            });
//        }
//        [TestPriority(0)]
//        [Trait("Category", nameof(GlobalTests))]
//        [Fact(DisplayName = nameof(ManagedCreationValuedCollection))]
//        public void ManagedCreationValuedCollection()
//        {
//            TestBody((session, ctx) =>
//            {
//                var user = new User() { Birthday = new DateTime(1988, 1, 30), Name = "Gianmaria" };
//                var user1 = new User() { Birthday = new DateTime(1989, 7, 26), Name = "Sofia" };
//                var user2 = new User() { Birthday = new DateTime(1989, 9, 28), Name = "Valentina" };
//                var book = new Book { Name = "Dune", Index = 0, Owner = user, Contributors = new List<User>() { user, user2 } };
//                var chapter1 = new Chapter { Name = "Capitolo 1", Index = 0, Book = book, Owner = user, Contributors = new List<User>() { user, user2 } };
//                book.Chapters = new List<Chapter>() { chapter1 };

//                user.Friends.Add(new Friend() { Destination = user1, MeetingDay= DateTime.Now, Score = 0.99 });
//                user.Friends.Add(new Friend() { Destination = user2, MeetingDay = DateTime.Now, Score = 1 });
//                user.BestFriend = new Friend() { Destination = user2, MeetingDay = DateTime.Now, Score = 10 };

//                ctx.Add(book);

//                ctx.SaveChanges();

//                //user.Friends[0].Score = 0.8;
//                //user2.Friends.Add(new Friend() { Source = user1, MeetingDay = DateTime.Now, Score = 1});

//                //ctx.SaveChanges(session);

//                //Chapter chapterQ = ctx.Query<Chapter>(session, k => 
//                //{
//                //    k.Include(p => p.Book).Include(p => p.Contributors);
//                //    k.Include(p => p.Book).Include(p => p.Owner);
//                //    k.Include(p => p.Owner);
//                //}).First(p => p.Id > 0);

//                Book bookQ = ctx.Query<Book>(k =>
//                {
//                    k.Include(p => p.Contributors).Include<Friend,User>(p=>p.Friends).Include(p=>p.OwnedContents);
//                    k.Include(p => p.Owner).Include<Friend, User>(p => p.BestFriend);
//                }).First(p => p.Id > 0);

//                //Assert.Equal(2, bookQ.Chapters.Count());
//                //Assert.Equal(bookQ.Id, bookQ.Chapters.First().Book.Id);
//                //Assert.Equal(bookQ.Contributors.First().Id, bookQ.Chapters.First().Contributors.First().Id);
//                //Assert.Equal(bookQ, bookQ.Chapters.First().Book);

//                ctx.Remove(book);
//                ctx.Remove(user);
//                ctx.Remove(user1);
//                ctx.Remove(user2);

//                ctx.SaveChanges();
//            });
//        }
//    }
//}
