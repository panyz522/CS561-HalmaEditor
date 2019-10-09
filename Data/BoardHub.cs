using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class BoardHub
    {
        public Dictionary<string, BoardManager> boardManagers = new Dictionary<string, BoardManager>();

        public BoardHub()
        {

        }

        public void Register(BoardManager manager)
        {
            boardManagers[manager.LinkedFilePath] = manager;
        }

        public void Unregister(string linkedFilePath)
        {
            if (linkedFilePath != null && boardManagers.ContainsKey(linkedFilePath))
                boardManagers.Remove(linkedFilePath);
        }
    }
}
