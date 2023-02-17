#pragma warning disable SA1101

namespace DsBot.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class DefaultPrefixEmbedBuilder : Discord.EmbedBuilder
{
    public DefaultPrefixEmbedBuilder()
    {
        WithColor(new Discord.Color((uint)0xBFA3D6));
    }
}
