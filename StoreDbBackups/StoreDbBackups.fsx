open System
open System.IO

type Result<'failure> = 
    | Success
    | Failure of 'failure

let getDaysToKeep (numberOfDaysToKeep : uint32) () = 
    [ 0..(int numberOfDaysToKeep) ]
    |> List.map (fun i -> DateTime.Today.AddDays(-(float i)))
    |> Set.ofList

let getFoldersToDelete getDaysToKeep backupRootPath (folderFormat : string) () = 
    let mapDateToFolderPath (date : DateTime) = Path.Combine(backupRootPath, date.ToString(folderFormat))
    let allFoldersFromRoot = Directory.GetDirectories(backupRootPath) |> Set.ofArray
    let foldersToKeep = getDaysToKeep() |> Set.map mapDateToFolderPath
    allFoldersFromRoot - foldersToKeep

let deleteFolder folderPath = 
    try 
        Directory.Delete(folderPath, true)
        Success
    with ex -> Failure ex.Message

let deleteFolders deleteFolder foldersToDeletePaths = 
    let accumulateResult allResults result = 
        match allResults, result with
        | Failure list, Failure error -> Failure(list @ [ error ])
        | Failure list, _ -> Failure list
        | _, Failure error -> Failure([ error ])
        | _ -> Success
    foldersToDeletePaths
    |> Seq.map deleteFolder
    |> Seq.fold accumulateResult Success

let deleteOldBackups deleteFolders getFoldersToDelete () = getFoldersToDelete() |> deleteFolders
// config
let numberOfDaysToKeep = 1u
let backupRootPath = "C:\Temp"
let folderFormat = "yyyy-MM-dd"
// composição
let getDaysToKeep' = getDaysToKeep numberOfDaysToKeep
let getFoldersToDelete' = getFoldersToDelete getDaysToKeep' backupRootPath folderFormat
let deleteFolders' = deleteFolders deleteFolder
let deleteOldBackups' = deleteOldBackups deleteFolders' getFoldersToDelete'

let storeBackupsAndCleanOldOnes deleteOldBackups copyFilesToBackupFolder notifyResult () = 
    [ deleteOldBackups()
      copyFilesToBackupFolder() ]
    |> notifyResult
