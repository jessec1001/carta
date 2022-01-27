import { useCallback, useEffect, useMemo, useState } from "react";

/** Represents the description for a loading stage. */
interface ILoadingStageDescriptor {
  /** The text associated with the stage. */
  text?: string;
  /** The weight of the stage. This changes how progress in this stage affects total progress. */
  weight?: number;
  /** A callback function that is called when a loading stage is started. */
  callback?: () => void;
}
/** Represents a loading stage with details filled in. */
interface ILoadingStage extends ILoadingStageDescriptor {
  complete: boolean;
  progress: number;
  weight: number;
}

/**
 * Manages the status of a sequence of loading stages. Each loading stage can have progress and completeness assigned to
 * it. The stages are assumed to progress in order, and once a loading stage is complete, the next stage is started. The
 * progress of all stages is calculated as a weighted average of the progress of all previous stages.
 * @param stageDescriptors The descriptors for the loading stages.
 * @returns A set of functions that can be used to start, update, complete, and get the status of the loading stages.
 */
const useLoading = (stageDescriptors: ILoadingStageDescriptor[]) => {
  // We take what the caller passes in as stage descriptors and convert it to actual stages.
  const [stages, setStages] = useState<ILoadingStage[]>(
    stageDescriptors.map(({ text, weight, callback }) => ({
      text: text,
      weight: weight ?? 1.0,
      callback: callback,
      complete: false,
      progress: 0.0,
    }))
  );

  // Define functions that can be called to manage the loading state.
  const findStage = useCallback(
    (index: number): [ILoadingStage | undefined, number] => [
      stages[index],
      index,
    ],
    [stages]
  );
  const findCurrentStage = useCallback((): [
    ILoadingStage | undefined,
    number
  ] => {
    const index = stages.findIndex((stage) => !stage.complete);
    const stage = index >= 0 ? stages[index] : undefined;
    return [stage, index >= 0 ? index : stages.length];
  }, [stages]);
  const completeStage = useCallback((index: number) => {
    setStages((stages) => {
      // Update the stage to be completed.
      let stage = stages[index];
      if (stage) stage = { ...stage, complete: true, progress: 1.0 };

      // Get the next stage to be completed and kick off its callback.
      const nextStage = stages[index + 1];
      if (nextStage) nextStage.callback?.();

      // Splice the updated stage into the array.
      if (stages[index])
        stages = [...stages.slice(0, index), stage, ...stages.slice(index + 1)];
      return stages;
    });
  }, []);
  const progressStage = useCallback((index: number, progress: number) => {
    setStages((stages) => {
      // Update the stage to be progressed.
      let stage = stages[index];
      if (stage) stage = { ...stage, progress: progress, complete: false };

      // Splice the updated stage into the array.
      if (stages[index])
        stages = [...stages.slice(0, index), stage, ...stages.slice(index + 1)];
      return stages;
    });
  }, []);
  const computeTotalProgress = useCallback(() => {
    // Get the total weight and total weighted progress.
    const totalWeight = stages.reduce(
      (total, stage) => total + stage.weight,
      0
    );
    const totalProgress = stages.reduce(
      (total, stage) => total + stage.weight * stage.progress,
      0
    );

    // We return the ratio in case the total weight is not 1.
    return totalWeight === 0 ? 1 : totalProgress / totalWeight;
  }, [stages]);
  const isLoadingComplete = useMemo(() => {
    return stages.every((stage) => stage.complete);
  }, [stages]);

  // We kick off the initial stage.
  useEffect(() => {
    completeStage(-1);
  }, [completeStage]);

  return useMemo(
    () => ({
      findStage,
      findCurrentStage,
      completeStage,
      progressStage,
      computeTotalProgress,
      isLoadingComplete,
    }),
    [
      completeStage,
      findCurrentStage,
      findStage,
      progressStage,
      computeTotalProgress,
      isLoadingComplete,
    ]
  );
};

export default useLoading;
