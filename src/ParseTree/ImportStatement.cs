using System;
using System.Collections.Generic;
using System.Text;

namespace Vanilla.ParseTree
{
    internal class ImportStatement : Entity
    {
        public string Path { get; private set; }

        public ImportStatement(Token firstToken, string path) : base(firstToken)
        {
            this.Path = path;
        }
    }
}
