using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models;
using CPUT.Polyglot.NoSql.Models.Translator;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Logic.Core.Events
{
    public class CommandEvent : ICommandEvent
    {
        public Dictionary<int, CommandHandler> Events { get; set; }

        public CommandEvent()
        {
            Events = new Dictionary<int, CommandHandler>();
        }

        public void Add(int index, CommandHandler handler)
        {
            Events.Add(index, handler);
        }

        public Output Run(Query request)
        {
            Output handler = null;

            if (Events.ContainsKey((int)request.Command))
            {
                handler = Events[(int)request.Command].Execute(request.Tokens);
            }

            return handler;
        }
    }
}
