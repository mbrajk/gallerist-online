﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TeamJAMiN.DataContexts;
using TeamJAMiN.Controllers.GameControllerHelpers;
using TeamJAMiN.GalleristComponentEntities;
using TeamJAMiN.Models;
using TeamJAMiN.GalleristComponentEntities.Dtos;

namespace TeamJAMiN.Controllers
{
    public class GameController : Controller
    {
        // GET: Game List
        //[AllowAnonymous] We may want to allow anonymous to list games in the future
        [Authorize]
        public ActionResult List()
        {
            var userName = User.Identity.Name;

            using (var identityContext = new ApplicationDbContext())
            {
                var userId = identityContext.Users.First(m => m.UserName == userName).Id;
                using (var galleristContext = new GalleristComponentsDbContext())
                {
                    var allGames = galleristContext.Games.Where(m => !m.IsCompleted);
                    var myGames = allGames.Where(m => m.Players.Any(n => n.UserId == userId));

                    var allGamesList = allGames.Select(m => new GameDto
                    {
                        Url = "/Game/Play/" + m.Id,
                        Name = m.Name,
                        CurrentNumberOfPlayers = m.Players.Count,
                        MaxNumberOfPlayers = m.MaxNumberOfPlayers,
                        RemainingSlots = m.MaxNumberOfPlayers - m.Players.Count,
                        MaxTurnLength = m.TurnLength,
                        MaxTurnLengthString = m.TurnLength + " Minutes Per Turn",
                        PlayersString = m.Players.Count + " of " + m.MaxNumberOfPlayers + " players"
                    }).ToList();

                    var myGamesList = myGames.Select(m => new GameDto
                    {
                        Url = "/Game/Play/" + m.Id,
                        Name = m.Name,
                        CurrentNumberOfPlayers = m.Players.Count,
                        MaxNumberOfPlayers = m.MaxNumberOfPlayers,
                        RemainingSlots = m.MaxNumberOfPlayers - m.Players.Count,
                        MaxTurnLength = m.TurnLength,
                        MaxTurnLengthString = m.TurnLength + " Minutes Per Turn",
                        PlayersString = m.Players.Count + " of " + m.MaxNumberOfPlayers + " players"
                    }).ToList();

                    ViewBag.allGames = allGamesList;
                    ViewBag.myGames = myGamesList;
                    return View();
                }
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View(new Game());
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Game newGame)
        {
            if (ModelState.IsValid)
            {
                using (var galleristContext = new GalleristComponentsDbContext())
                {
                    newGame.CreateRandomSetup();
                    galleristContext.Games.Add(newGame);
                    using (var identityContext = new ApplicationDbContext())
                    {
                        //add me to the game
                        newGame.Players.Add(new Player { UserId = identityContext.Users.First(m => m.UserName == User.Identity.Name).Id });
                        newGame.FinalizeSetup();
                    }
                    galleristContext.SaveChanges();
                    return Redirect("/Game/List"); //redirect to actual game might be better for demo purposes
                }
            }
            else
            {
                return View(newGame);
            }
        }

        /// <summary>
        /// Takes you to an existing view for a game for which you are a member
        /// </summary>
        /// <returns>Game View</returns>
        [Authorize]
        public ActionResult Play(int id = 0)
        {
            using (var galleristContext = new GalleristComponentsDbContext())
            {
                using (var identityContext = new ApplicationDbContext())
                {
                    var gameResponse = GameManager.GetGame(id, User.Identity.Name, galleristContext, identityContext);

                    if (gameResponse.Success)
                    {
                        ViewBag.userName = User.Identity.Name;
                        return View(gameResponse.Game);
                    }
                    else
                    {
                        ViewBag.Message = gameResponse.Message;
                        ViewBag.Title = gameResponse.Title;
                        return View("GameError");
                    }
                }
            }
        }

        /// <summary>
        /// Joins an existing game and takes you to the view for that game.
        /// </summary>
        /// <returns>Existing game view or appropriate error</returns>
        [Authorize]
        [HttpPost]
        public ActionResult Join(int id = 0)
        {
            using (var galleristContext = new GalleristComponentsDbContext())
            {
                using (var identityContext = new ApplicationDbContext())
                {
                    var gameResponse = GameManager.GetGame(id, User.Identity.Name, galleristContext, identityContext);

                    if (gameResponse.Success)
                    {
                        gameResponse.Game.Players.Add(new Player { UserId = identityContext.Users.First(m => m.UserName == User.Identity.Name).Id });
                        ViewBag.userName = User.Identity.Name;
                        return View(gameResponse.Game);
                    }
                    else
                    {
                        ViewBag.Message = gameResponse.Message;
                        ViewBag.Title = gameResponse.Title;
                        return View("GameError");
                    }
                }
            }
        }
    }
}
