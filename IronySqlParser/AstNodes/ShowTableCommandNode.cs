﻿using System.Collections.Generic;

namespace IronySqlParser.AstNodes
{
    class ShowTableCommandNode : SqlCommandNode
    {
        public List<string> TableName { get; set; }

        public override void CollectInfoFromChild() => TableName = FindFirstChildNodeByType<IdNode>()?.Id;
    }
}