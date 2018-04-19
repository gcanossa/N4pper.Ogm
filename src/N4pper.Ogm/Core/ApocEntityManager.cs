﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using N4pper.Ogm.Design;
using N4pper.Ogm.Entities;
using N4pper.QueryUtils;
using Neo4j.Driver.V1;
using OMnG;

namespace N4pper.Ogm.Core
{
    public class ApocEntityManager : CypherEntityManager
    {
        public override IEnumerable<IOgmEntity> CreateNodes(IStatementRunner runner, IEnumerable<Tuple<IOgmEntity, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"CALL apoc.create.node({row}.{nameof(NodeEntity.Labels)}, {row}.{nameof(NodeEntity.Properties)}) yield node AS {m} ");
                sb.Append($"SET {m}.{nameof(IOgmEntity.EntityId)}=id({m}) ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new NodeEntity(p.Item1, false, excludePorperties: p.Item2).ToPropDictionary()).ToList() }).ToList();
            }
        }

        public override IEnumerable<IOgmConnection> CreateRels(IStatementRunner runner, IEnumerable<Tuple<IOgmConnection, IEnumerable<string>>> entities)
        {
            runner = runner ?? throw new ArgumentNullException(nameof(runner));
            entities = entities ?? throw new ArgumentNullException(nameof(entities));

            entities = entities.Where(p => p?.Item1 != null);

            if (entities.Count() == 0)
                return null;

            IGraphManagedStatementRunner mgr = (runner as IGraphManagedStatementRunner) ?? throw new ArgumentException("The statement must be decorated.", nameof(runner));

            using (ManagerAccess.Manager.ScopeOMnG())
            {
                Symbol row = new Symbol();
                Symbol m = new Symbol();
                Symbol s = new Symbol();
                Symbol d = new Symbol();

                StringBuilder sb = new StringBuilder();

                sb.Append($"UNWIND $batch AS {row} ");
                sb.Append($"MATCH ({s} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.SourceId)}}})");
                sb.Append($"MATCH ({d} {{{nameof(IOgmEntity.EntityId)}:{row}.{nameof(RelEntity.DestinationId)}}})");
                sb.Append($"CALL apoc.create.relationship({s}, {row}.{nameof(RelEntity.Label)}, {row}.{nameof(RelEntity.Properties)}, {d}) yield rel AS {m} ");
                sb.Append($"SET {m}.{nameof(IOgmEntity.EntityId)}=id({m}) ");
                sb.Append($"RETURN {m}");

                return runner.ExecuteQuery<IOgmEntity>(sb.ToString(), new { batch = entities.Select(p => new RelEntity(p.Item1, p.Item1.Source.EntityId.Value, p.Item1.Destination.EntityId.Value, false, excludePorperties: p.Item2).ToPropDictionary()).ToList() }).ToList().Select(p=>p as IOgmConnection);
            }
        }
    }
}
