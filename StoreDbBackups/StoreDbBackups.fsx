open System
open System.IO

type Result<'success, 'failure> = 
    | Success of 'success
    | Failure of 'failure

let getDaysToKeep (numberOfDaysToKeep : uint32) () = 
    [ 0..(int numberOfDaysToKeep) ]
    |> List.map (fun i -> DateTime.Today.AddDays(-(float i)))
    |> Set.ofList

let getFoldersToDelete getDaysToKeep backupRootPath () = 
    let mapDateToFolderPath (date : DateTime) = Path.Combine(backupRootPath, date.ToString("dd-MM-yyyy"))
    let allFoldersFromRoot = Directory.GetDirectories(backupRootPath) |> Set.ofArray
    let foldersToKeep = getDaysToKeep() |> Set.map mapDateToFolderPath
    allFoldersFromRoot - foldersToKeep

let deleteOldBackups deleteFolders getFoldersToDelete () = getFoldersToDelete() |> deleteFolders

let storeBackupsAndCleanOldOnes deleteOldBackups copyFilesToBackupFolder logResult () = 
    [ deleteOldBackups()
      copyFilesToBackupFolder() ]
    |> logResult