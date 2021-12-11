using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace {0}
{{
    public partial class {1}
    {{
        private delegate ValueTask InternalHandle(ArraySegment<byte> buffer{4});

        private readonly Dictionary<int, Dictionary<int, InternalHandle>> _handlers = new();

        public {1}() 
        {{
{2}
        }}

        public bool Contains(int opcode)
        {{
            return Contains(opcode, 0);
        }}

        public bool Contains(int opcode, int group)
        {{
            return _handlers.ContainsKey(group) && _handlers[group].ContainsKey(opcode);
        }}

        public ValueTask Handle(int opcode, ArraySegment<byte> buffer{4})
        {{
            return _handlers[0][opcode](buffer{5});
        }}

        public ValueTask Handle(int opcode, int group, ArraySegment<byte> buffer{4})
        {{
            return _handlers[group][opcode](buffer{5});
        }}

{3}
    }}
}}
