﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamJAMiN.Controllers.GameControllerHelpers;
using TeamJAMiN.GalleristComponentEntities;

namespace TeamJAMiN.Controllers.GameLogicHelpers
{
    public static class TurnManager
    {
        public static void SetCurrentAction(this GameTurn turn, GameActionState state, string location)
        {
            if (turn.CurrentAction != null)
            {
                turn.AddCompletedAction(turn.CurrentAction);
            }
            turn.CurrentAction = new GameAction { State = state, Location = location, IsExecutable = true };
        }

        public static void AddPendingAction(this GameTurn turn, GameAction action)
        {
            var temp = turn.PendingActions;
            temp.Add(action);
            turn.PendingActions = temp;
        }
        public static void AddPendingActions(this GameTurn turn, List<GameActionState> actions, GameActionPriority priority, bool isExecutable)
        {
            var temp = turn.PendingActions;
            foreach ( GameActionState state in actions)
            {
                var action = new GameAction { State = state, Priority = priority, IsExecutable = isExecutable };
                temp.Add(action);
            }
            turn.PendingActions = temp;
        }

        public static void AddPendingActions(this GameTurn turn, List<GameActionState> actions, GameAction parent, GameActionPriority priority, bool isExecutable)
        {
            var temp = turn.PendingActions;
            foreach (GameActionState state in actions)
            {
                var action = new GameAction { State = state, Parent = parent, Priority = priority, IsExecutable = isExecutable };
                temp.Add(action);
            }
            turn.PendingActions = temp;
        }

        public static void RemovePendingAction(this GameTurn turn, GameAction action)
        {
            var temp = turn.PendingActions;
            temp.Remove(action);
            turn.PendingActions = temp;
        }

        public static void RemovePendingAction(this GameTurn turn, GameActionState state)
        {
            var temp = turn.PendingActions;
            var action = temp.FirstOrDefault(a => a.State == state);
            if( action != null)
            {
                temp.Remove(action);
            }
            turn.PendingActions = temp;
        }

        public static void RemoveAllSiblingActions(this GameTurn turn, GameActionState state)
        {
            var temp = turn.PendingActions;
            var action = temp.FirstOrDefault(a => a.State == state);
            if (action != null)
            {
                if (action.Priority == GameActionPriority.OptionalExclusive)
                {
                    temp.RemoveAll(a => a.Priority == GameActionPriority.OptionalExclusive && a.Parent.State == action.Parent.State);
                }
                else
                {
                    temp.Remove(action);
                }
            }
            turn.PendingActions = temp;
        }

        public static void RemoveAllSiblingActions(this GameTurn turn, GameAction action)
        {
            var temp = turn.PendingActions;
            if (action != null)
            {
                if (action.Priority == GameActionPriority.OptionalExclusive)
                {
                    temp.RemoveAll(a => a.Priority == GameActionPriority.OptionalExclusive && a.Parent == action.Parent);
                }
                else
                {
                    temp.Remove(action);
                }
            }
            turn.PendingActions = temp;
        }

        public static void AddCompletedAction(this GameTurn turn, GameAction action)
        {
            if(action.IsComplete == false)
            {
                action.IsComplete = true;
                var temp = turn.CompletedActions;
                temp.Add(action);
                turn.CompletedActions = temp;
            }
        }

        public static void SetupFirstTurn(this Game newGame)
        {
            var firstTurn = new GameTurn { TurnNumber = 0, Type = GameTurnType.Setup, CurrentAction = new GameAction { State = GameActionState.GameStart } };
            newGame.Turns.Add(firstTurn);
            var next = new GameAction { State = GameActionState.Pass };
            (new ActionContextInvoker(newGame)).DoActionSingle(next);

        }
        public static void SetupNextTurn(this Game game)
        {
            var oldTurn = game.CurrentTurn;
            if(oldTurn.KickedOutPlayer != null)
            {
                game.SetupKickedOutTurn(oldTurn);
            }
            else
            {
                game.SetupTurn(oldTurn);
            }
        }

        public static void SetupKickedOutTurn(this Game game, GameTurn oldTurn)
        {
            //todo add turnstart action state, then add completed turn start action
            //todo add chooselocation to pending actions
            var newTurn = new GameTurn { Game = game, TurnNumber = oldTurn.TurnNumber + 1, CurrentPlayer = oldTurn.KickedOutPlayer };
            game.CurrentPlayerId = oldTurn.KickedOutPlayer.Id;
            game.Turns.Add(newTurn);
            newTurn.StartTurn();
        }

        public static void SetupTurn(this Game game, GameTurn oldTurn)
        {
            game.SetNextPlayer();
            var newTurn = new GameTurn { Game = game, TurnNumber = oldTurn.TurnNumber + 1, CurrentPlayer = game.CurrentPlayer };
            game.Turns.Add(newTurn);
            newTurn.StartTurn();
        }

        public static void StartTurn(this GameTurn turn)
        {
            var startAction = new GameAction { State = GameActionState.ChooseLocation };
            turn.CurrentAction = startAction;
            var invoker = new ActionContextInvoker(turn.Game);
            invoker.DoActionSingle(startAction);
        }
    }
}