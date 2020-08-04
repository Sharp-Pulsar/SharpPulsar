﻿using k8s.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Deployment.Kubernetes.Builders
{
    public class RoleBuilder
    {
        private V1Role _role;
        public RoleBuilder()
        {
            _role = new V1Role
            {
                Metadata = new V1ObjectMeta()
            };
        }
        public RoleBuilder Name(string name, string @namespace)
        {
            _role.Metadata.Name = name;
            _role.Metadata.NamespaceProperty = @namespace;
            return this;
        }
        public RoleBuilder Labels(IDictionary<string, string> labels)
        {
            _role.Metadata.Labels = labels;
            return this;
        }
        public RoleBuilder AddRule(string[] apiGroups, string[] resources, string[] verbs, string[] resourceNames)
        {
            _role.Rules.Add(new V1PolicyRule
            {
                ApiGroups = apiGroups,
                Resources = resources,
                Verbs = verbs,
                ResourceNames = resourceNames
            });
            return this;
        }
        public V1Role Build()
        {
            return _role;
        }
    }
}