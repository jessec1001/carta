import { FunctionComponent, useEffect, useState } from "react";
import { useStoredState } from "hooks";
import { Nav, NavItem, NavLink } from "reactstrap";
import { DataApi } from "library/api";
import { HeightScroll } from "components/ui/layout/scroll/HeightScroll";

import "./GraphOpenForm.css";
import { Section, Subsection } from "components/text";

interface GraphOpenFormProps {
  parameters?: Record<string, any>;

  onOpenGraph?: (
    source: string,
    resource: string,
    parameters?: Record<string, any>
  ) => void;
}

const GraphOpenForm: FunctionComponent<GraphOpenFormProps> = ({
  parameters,
  onOpenGraph,
}) => {
  const [resources, setResources] = useState<
    Record<string, string[] | null> | undefined
  >(undefined);
  const hyperthoughtKey = useStoredState("", "hyperthoughtKey")[0];

  useEffect(() => {
    const fetchResources = async (source: string) => {
      try {
        const resources = await DataApi.getResourcesAsync({ source });
        if (Array.isArray(resources)) {
          setResources((prevResources) => {
            const nextResources = { ...prevResources };
            nextResources[source] = resources;
            return nextResources;
          });
        }
      } catch {
        setResources((prevResources) => {
          const nextResources = { ...prevResources };
          delete nextResources[source];
          return nextResources;
        });
      }
    };
    const fetchSources = async () => {
      const sources = await DataApi.getSourcesAsync();
      setResources((prevResources) => {
        const nextResources = { ...prevResources };
        sources.forEach((source) => {
          if (!Array.isArray(nextResources)) {
            nextResources[source] = null;
          }
          fetchResources(source);
        });
        return nextResources;
      });
    };

    fetchSources();
  }, [hyperthoughtKey]);

  const handleOpenGraph = (source: string, resource: string) => {
    if (onOpenGraph) {
      onOpenGraph(source, resource, parameters);
    }
  };

  return (
    <HeightScroll className="form-graphOpen">
      <Section title="Open Graph">
        {resources &&
          Object.keys(resources).map((source) => {
            if (resources) {
              if (resources[source] === null) {
                return (
                  <Subsection key={source} title={source}>
                    Loading
                  </Subsection>
                );
              } else if (resources[source]!.length > 0) {
                return (
                  <Subsection key={source} title={source}>
                    <Nav vertical>
                      {resources![source]?.map((resource) => (
                        <NavItem key={resource}>
                          <NavLink
                            href="#"
                            onClick={() => handleOpenGraph(source, resource)}
                          >
                            {resource}
                          </NavLink>
                        </NavItem>
                      ))}
                    </Nav>
                    {!resources![source] && <p>Loading</p>}
                  </Subsection>
                );
              } else {
                return null;
              }
            } else return null;
          })}
        {!resources && <h3>Loading</h3>}
      </Section>
    </HeightScroll>
  );
};

export default GraphOpenForm;
export type { GraphOpenFormProps };
