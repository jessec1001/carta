## Summary

<!--
    Briefly summarize (in 2-4 sentences) the API/persistence change or addition
    that should be implemented. REQUIRED
 -->

### API Endpoints

<!-- 
    Describe the API endpoints that make up the proposed back-end changes if
    applicable. This should include information on:
    - url route
    - route parameters
    - query parameters
    - request body
    - response body
    RECOMMENDED

    This should be able to be used as a task list of items to implement.
 -->

- [ ] `GET /api/test/{testId}`

    Description of API endpoint 1.
    - [ ] Requirement A
    - [ ] Requirement B

    __Route__

    - [ ] `testId` - Description of test ID. Required.

    __Reponse__
    ```js
    {
        id: {ID},
        name: "sample output",
        data: [4, 3, 2, 1]
    }
    ```

- [ ] `POST /api/foo/things?size=lessthan({size})&version?={versionNumber}`

    Description of API endpoint 2.

    __Parameters__

    - [ ] `size` - Description of size. Required.
    - [ ] `version` - Description of version. Optional.

    __Body__

    A `Thing` data structure. More description goes here.

    __Response__

    `null`

### Data Structures

<!--
    Describe the data structures that need to be added or changed to implement
    the proposed back-end changes if applicable. OPTIONAL

    This should be able to be used as a task list of items to implement.
 -->

- [ ] Data Structure 1

    Description of data structure 1.
    - [ ] Requirement A
    - [ ] Requirement B
- [ ] Data Structure 2

    Description of data structure 2.

### Algorithms

<!--
    Describe the algorithms that need to be added or changed to implement
    the proposed back-end changes if applicable. This is recommended for
    particularly complex algorithms. OPTIONAL

    This should be able to be used as a task list of items to implement.
 -->

- [ ] Data Structure 1

    Description of data structure 1.
    - [ ] Requirement A
    - [ ] Requirement B

    __Inputs__
    - `foo` - Describe foo. Required.
    - `bar` - Describe bar. Required.

    __Outputs__
    - `baz` - Describe baz.

- [ ] Data Structure 2

    Description of data structure 2.

    __Outputs__
    - `qux` - Describe qux.

## Use Cases

<!--
    Give some descriptions of how a user might consume the API in order to
    accomplish common tasks.

    This should be used to implement a collection of comprehensive tests for
    the functionality.
 -->

- [ ] An authenticated user creates a workflow and adds it to a workspace.
    1. The user retrieves all existing workspaces by calling `GET /api/workspace`.
    2. The user uses the ID of a particular retrieved workspace, `workspaceId = response[index].id`.
    3. The user creates a workflow by calling `POST /api/workflow + BODY Workflow`.
    4. The user uses the ID assigned to the created workflow, `workflowId = response.id`.
    5. The user adds the workflow to the workspace by calling `POST api/workspace/{workspaceId}/workflows/{workflowId}`.
    6. The user can verify that the workflow was added by calling `GET api/workspace/{workspaceId}/workflows`. 

<!-- 
    !!! IMPORTANT !!!
    DO NOT MODIFY THE ISSUE BELOW THIS LINE
 -->

/label ~"Type: Functionality" ~"Category: Back-End"